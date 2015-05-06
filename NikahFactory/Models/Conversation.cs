using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NikahFactory.Models
{
    public class Conversation
    {
        public int ConversationId { get; set; }
        public DateTime Last { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}