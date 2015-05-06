using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NikahFactory.APIRequestModels
{
    public class TokenRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}