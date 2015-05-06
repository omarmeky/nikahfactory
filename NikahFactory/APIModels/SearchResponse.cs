using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NikahFactory.APIModels
{
    public class SearchResponse
    {
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public int Age { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Heading { get; set; }
        public bool Alerted { get; set; }
    }
}