using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NikahFactory.APIModels
{
    public class ReplyRequest
    {
        public int ConversationId { get; set; }
        public int Receiver { get; set; }
        public string Message { get; set; }
    }
}