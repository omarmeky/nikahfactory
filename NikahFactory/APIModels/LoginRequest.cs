using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NikahFactory.APIModels
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}