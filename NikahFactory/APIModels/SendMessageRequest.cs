using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NikahFactory.APIModels
{
    public class SendMessageRequest
    {
        public int UserID { get; set; }
        public string Message { get; set; }
    }
}