using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AmazonApi.Models
{
    public class AmazonSearchResult
    {
        public List<AmazonProduct> AmazonProducts { get; set; }

        public int TotalPages { get; set; }

        public int CurrentPageNumber { get; set; }

        public string Query { get; set; }
    }
}