using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NikahFactory.Models
{
    public class Alert
    {
        public int AlertId { get; set; }
        public bool New { get; set; }
        public DateTime AlertDateTime { get; set; }
        public virtual User Alerter { get; set; }
        public virtual User Alerted { get; set; } 
    }
}