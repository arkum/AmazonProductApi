using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmazonApi.Models
{
    public class AmazonProduct
    {
        public AmazonProduct(string asin)
        {
            ASIN = asin;
        }
        public string ASIN { get; set; }

        public string FormattedPrice { get; set; }

        public string Title { get; set; }

        public string[] Features { get; set; }

        public string LargeImage { get; set; }

        public string MediumImage { get; set; }

        public string SmallImage { get; set; }

        public string OfferListingId { get; set; }
        public IEnumerable<ProductImage> ImageSets { get; set; }

        public string Description { get; set; }
        public string Availability { get; set; }

    }
}
