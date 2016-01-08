using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AmazonApi.Models
{
    public class AmazonShoppingCart
    {
        
        public string Asin { get; set; }

        public string Title { get; set; }


        public string CartId { get; set; }

        public string Hmac { get; set; }

        public string PurchaseUrl { get; set; }

        public string CartItemId { get; set; }

        public IEnumerable<AmazonProduct> AmazonProducts { get; set; }

        public string OfferListingId { get; set; }
        public string Error { get; set; }
    }
}