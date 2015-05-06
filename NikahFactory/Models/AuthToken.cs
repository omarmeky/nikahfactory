using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NikahFactory.Models
{
    public class AuthToken
    {
        public int AuthTokenId { get; set; }
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public User User { get; set; }
    }
}