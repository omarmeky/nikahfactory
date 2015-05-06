using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NikahFactory.APIModels
{
    public class MessageResponse
    {
        public int MessageID { get; set; }
        public string Sender { get; set; }
        public string Gender { get; set; }
        public bool Me { get; set; }
        public string Body { get; set; }
    }
}