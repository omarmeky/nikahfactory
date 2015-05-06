using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NikahFactory.Models;

namespace NikahFactory.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string CustomerId { get; set; }
        public string CreditCard { get; set; }
        public bool Active { get; set; }
        public bool Paused { get; set; }
        public bool Unpaid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Gender { get; set; }
        public DateTime Birthday { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Heading { get; set; }
        public string Bio { get; set; }
        public virtual Guardian Guardian { get; set; }
        public virtual ICollection<Conversation> Conversations { get; set; }
    }
}