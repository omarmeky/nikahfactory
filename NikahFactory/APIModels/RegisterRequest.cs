using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NikahFactory.APIModels
{
    public class RegisterRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Gender { get; set; }
        public string GuardianEmail { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public long Birthday { get; set; }
        public string CreditCard { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        public string SecurityCode { get; set; }
    }
}
