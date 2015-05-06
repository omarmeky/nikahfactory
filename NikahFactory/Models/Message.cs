using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NikahFactory.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public DateTime DateTime { get; set; }
        public virtual User Sender { get; set; }
        public virtual User Receiver { get; set; }
        public string Body { get; set; }
        public bool Unread { get; set; }
    }
}