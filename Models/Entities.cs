using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AttendanceWebApi.Models
{
    [Table("employee")]
    public class Employee
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string contactnumber { get; set; }
        public string address { get; set; }
        public string birthdate { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }


    [Table("timeinout")]
    public class Timeinout
    {
        [Key]
        public int id { get; set; }
        public int employee_id { get; set; }
        public DateTime? timein { get; set; }
        public DateTime? timeout { get; set; }

    }

    [Table("adminaccount")]
    public class Adminaccount
    {
        [Key]
        public int id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string name { get; set; }
    }








}