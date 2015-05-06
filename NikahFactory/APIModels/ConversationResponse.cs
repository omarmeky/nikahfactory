using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NikahFactory.APIModels
{
    public class ConversationResponse
    {
        public int ConversationID { get; set; }
        public int UserID { get; set; }
        public string User { get; set; }
        public string Gender { get; set; }
        public MessageResponse[] Messages { get; set; }
    }
}