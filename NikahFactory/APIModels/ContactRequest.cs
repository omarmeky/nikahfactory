using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NikahFactory.APIModels
{
    public class ContactRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public override string ToString()
        {
            return Name + " (" + Email + ")" + "\n" + Message;
        }
    }
}