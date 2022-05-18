using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace myproject.Models
{
    public class Data
    {
        public int id { get; set; }
        public string light { get; set; }
        public string status { get; set; }
        [DataType(DataType.Date)]
        public DateTime create_date { get; set; }
        [DataType(DataType.Date)]
        public DateTime update_date { get; set; }
        public bool is_delete { get; set; }
        public string formatdatetime(DateTime date) {
            return date.ToString("dd/MM/yyyy HH:mm");
        }
    }
}