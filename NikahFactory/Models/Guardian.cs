using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NikahFactory.Models
{
    public class Guardian
    {
        public int GuardianId { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public bool Verified { get; set; }
    }
}
