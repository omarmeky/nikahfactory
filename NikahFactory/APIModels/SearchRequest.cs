using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NikahFactory.APIModels
{
    public class SearchRequest
    {
        public string[] Countries { get; set; }
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
    }
}