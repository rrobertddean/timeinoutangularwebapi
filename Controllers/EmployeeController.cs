using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;
using AttendanceWebApi.Models;
using AttendanceWebApi.Context;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Text;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;
using OfficeOpenXml;

namespace AttendanceWebApi.Controllers
{

    public class Record
    {
        public string FName { get; set; }
        public string LName { get; set; }
        public string Address { get; set; }
    }

    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class EmployeeController : ApiController
    {

        DatabaseContext db;

        public EmployeeController()
        {
            db = new DatabaseContext();
        }

        [Route("api/employee/timeinauthenticate")]
        [HttpPost]
        public string timeinauthenticate(Employee employee)
        {
            DateTime startDateTime = DateTime.Today; //Today at 00:00:00
            DateTime endDateTime = DateTime.Today.AddDays(1).AddTicks(-1); //Today at 23:59:59

            var uname = employee.username;
            var pword = employee.password;

            var result = from emp in db.employees
                         join ti in db.timeinouts
                         on emp.id equals ti.employee_id
                         where emp.username == uname && emp.password == pword && (ti.timein >= startDateTime && ti.timein <= endDateTime)
                         select new { emp.id };


            if (result.Count() > 0)
            {
                return "-1"; //employee already timed in
            }
            else
            {
                int empid = 0;
                var resEmp = from emp in db.employees where emp.username == uname && emp.password == pword select new { emp.id };

                if (resEmp.Count() > 0)
                {
                    foreach (var item in resEmp)
                    {
                        empid = item.id;
                    }

                    Timeinout objTi = new Timeinout();
                    objTi.employee_id = empid;
                    objTi.timein = DateTime.Now;
                    db.timeinouts.Add(objTi);
                    db.SaveChanges();

                    return "0";
                }
                else
                {
                    return "-2"; //employee not exists
                }
               
            }

        }

        [HttpGet]
        [Route("api/employee/loadTimeIn")]
        public string loadTimeIn()
        {

            var result = from emp in db.employees
                         join ti in db.timeinouts
                         on emp.id equals ti.employee_id
                         orderby ti.id descending
                         select new { ti.timein, emp.name };

            string json = JsonConvert.SerializeObject(result);
            return json;
        }

        [HttpGet]
        [Route("api/employee/loadTimeInToday")]
        public string loadTimeInToday()
        {
            DateTime startDateTime = DateTime.Today; //Today at 00:00:00
            DateTime endDateTime = DateTime.Today.AddDays(1).AddTicks(-1); //Today at 23:59:59

            var result = from emp in db.employees
                         join ti in db.timeinouts
                         on emp.id equals ti.employee_id
                         where (ti.timein >= startDateTime && ti.timein <= endDateTime)
                         select new { ti.timein,emp.name };

            string json = JsonConvert.SerializeObject(result);
            return json;

        }

        [HttpPost]
        [Route("api/employee/searchDateTimeIn")]
        public string searchDateTimeIn(dynamic model)
        {
            DateTime frDate = model.frDate;
            DateTime toDate = model.toDate;

            var result = from emp in db.employees
                         join ti in db.timeinouts
                         on emp.id equals ti.employee_id
                         where (DbFunctions.TruncateTime(ti.timein) >= DbFunctions.TruncateTime(frDate) 
                         && DbFunctions.TruncateTime(ti.timein) <= DbFunctions.TruncateTime(toDate))
                         select new { ti.timein, emp.name };

            string json = JsonConvert.SerializeObject(result);
            return json;

        }

        [HttpPost]
        [Route("api/employee/loginadminauthenticate")]
        public string loginadminauthenticate(Adminaccount adminaccount)
        {
            var uname = adminaccount.username;
            var pword = adminaccount.password;

            var result = from adm in db.adminaccounts
                         where adm.username == uname && adm.password == pword
                         select adm;

            string json = JsonConvert.SerializeObject(result);
            return json;

        }

        [HttpGet]
        [Route("api/employee/loadEmployees")]
        public string loadEmployees()
        {

            var result = from emp in db.employees
                         select emp;
            string json = JsonConvert.SerializeObject(result);
            return json;

        }

        [HttpPut]
        [Route("api/employee/putEmployee/{id}")]
        public string putEmployee(int id, Employee employee)
        {
            if (id != employee.id)
            {
                return "-1"; //Bad Request
            }

            db.Entry(employee).State = System.Data.Entity.EntityState.Modified;

            try
            {
                db.SaveChanges();
                return "1";
            }
            catch (DbUpdateConcurrencyException)
            {
                
            }

            return "0";

        }

        [HttpPost]
        [Route("api/employee/postEmployee")]
        public string postEmployee(Employee employee)
        {
            try {
                db.employees.Add(employee);
                db.SaveChanges();
                return "1";
            } catch (Exception e) {
                return "-1";
            }

        }


        [HttpGet]
        [Route("api/employee/downloadAttendance")]
        public void downloadAttendance(DateTime from, DateTime to)
        {
            DateTime frdate = from;
            DateTime todate = to;

            var result = from emp in db.employees
                         join ti in db.timeinouts
                         on emp.id equals ti.employee_id
                         where (DbFunctions.TruncateTime(ti.timein) >= DbFunctions.TruncateTime(frdate)
                         && DbFunctions.TruncateTime(ti.timein) <= DbFunctions.TruncateTime(todate))
                         select new { ti.timein, emp.name };


            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");

            Sheet.Cells["A1"].Value = "Name";
            Sheet.Cells["B1"].Value = "TimeIn";

            int row = 2;


            foreach (var item in result)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.name;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.timein;
                Sheet.Cells[string.Format("B{0}", row)].Style.Numberformat.Format = "yyyy-mm-dd";
                row++;
            }


            Sheet.Cells["A:AZ"].AutoFitColumns();
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            HttpContext.Current.Response.BinaryWrite(Ep.GetAsByteArray());
            HttpContext.Current.Response.End();
            

            //string bookPath_xls = @"D:\Robert\testexcel.csv";
            //string format = "csv";
            //string reqBook = bookPath_xls;
            //string bookName = "sample." + format.ToLower();

            ////converting Pdf file into bytes array  
            //var dataBytes = File.ReadAllBytes(reqBook);
            ////adding bytes to memory stream   
            //var dataStream = new MemoryStream(dataBytes);
            //return new eBookResult(dataStream, Request, bookName);


        }

    }


    public class eBookResult : IHttpActionResult
    {
        MemoryStream bookStuff;
        string PdfFileName;
        HttpRequestMessage httpRequestMessage;
        HttpResponseMessage httpResponseMessage;
        public eBookResult(MemoryStream data, HttpRequestMessage request, string filename)
        {
            bookStuff = data;
            httpRequestMessage = request;
            PdfFileName = filename;
        }
        public System.Threading.Tasks.Task<HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            httpResponseMessage = httpRequestMessage.CreateResponse(HttpStatusCode.OK);
            httpResponseMessage.Content = new StreamContent(bookStuff);
            //httpResponseMessage.Content = new ByteArrayContent(bookStuff.ToArray());  
            httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            httpResponseMessage.Content.Headers.ContentDisposition.FileName = PdfFileName;
            httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            return System.Threading.Tasks.Task.FromResult(httpResponseMessage);
        }
    }
}
