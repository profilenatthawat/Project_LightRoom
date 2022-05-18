using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace myproject.Models
{
    public class DB
    {
        public int id { get; set; }
        public string home { get; set; }
        
    }
}