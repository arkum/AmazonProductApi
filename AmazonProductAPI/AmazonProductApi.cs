using AmazonApi.Core;
using AmazonApi.Models;
using AmazonProductAdvtApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace AmazonApi
{
    public class AmazonProductApi : IAmazonProductApi
    {
        private string _awskey;
        private string _awsSecretKey;
        private string _destination;
        private string _xmlnamespace;
        private IDictionary<string, string> requestparameters;

        public AmazonProductApi(AwsConfiguration configuration)
        {
            this._awskey = configuration.AwsKey;
            this._awsSecretKey = configuration.AwsSecretKey;
            this._destination = configuration.Destination;
            this._xmlnamespace = configuration.XmlNamespace;
            requestparameters = new Dictionary<string, string>() { { "Service", "AWSECommerceService" }, { "Version", configuration.ApiVersion }, { "AssociateTag", configuration.AssociateTag } };
        }

        private async Task<XDocument> InvokeAmazonService(string signedUrl)
        {
            var response = await HttpWebRequest.Create(signedUrl).GetResponseAsync();
            var xDocument = XDocument.Load(response.GetResponseStream());
            XNamespace ns = _xmlnamespace;
            var isValid = xDocument.Descendants(ns + "IsValid").Select(p=>p.Value).First();
            var errors = xDocument.Descendants(ns + "Errors").Select(p=>p.Value).ToList();

            if (errors.Count() != 0 && isValid.Equals("False"))
                throw new AmazonException() { Messages = errors };
            
            return xDocument;
        }

        public async Task<IEnumerable<AmazonProduct>> ItemLookup(string[] productids, string responsegroup, Func<string, Task<XDocument>> retrievalfunc = null)
        {
            requestparameters["Operation"] = "ItemLookup";
            requestparameters["ItemId"] = string.Join(",", productids);
            requestparameters["ResponseGroup"] = responsegroup;
            string url = new SignedRequestHelper(_awskey, _awsSecretKey, _destination).Sign(requestparameters);
            XNamespace ns = _xmlnamespace;
            if (retrievalfunc == null)
                retrievalfunc = InvokeAmazonService;
            var xml = await retrievalfunc(url);
            var items = xml.Descendants(ns + "Item").Where((elem) => elem.Descendants(ns + "OfferListingId").Count() != 0)
                .Select((e) => e.ToAmazonProduct(ns));

            return items;
        }

        public async Task<AmazonProduct> ItemDetails(string ASIN, Func<string, Task<XDocument>> retrievalfunc = null)
        {
            requestparameters["Operation"] = "ItemLookup";
            requestparameters["ItemId"] = ASIN;
            requestparameters["ResponseGroup"] = "Images,ItemAttributes,Offers,EditorialReview,OfferFull";
            string url = new SignedRequestHelper(_awskey, _awsSecretKey, _destination).Sign(requestparameters);
            XNamespace ns = _xmlnamespace;
            if (retrievalfunc == null)
                retrievalfunc = InvokeAmazonService;
            var xml = await retrievalfunc(url);
            var items = xml.Descendants(ns + "Item").Where((elem) => elem.Descendants(ns + "OfferListingId").Count() != 0)
                .Select((e) => e.ToAmazonProduct(ns));

            return items.First();
        }


        public async Task<IEnumerable<AmazonProduct>> SimilarItems(string ASIN, Func<string, Task<XDocument>> retrievalfunc = null)
        {
            requestparameters["Operation"] = "SimilarityLookup";
            requestparameters["ItemId"] = ASIN;
            requestparameters["ResponseGroup"] = "Images,Offers,OfferSummary,Small";
            string url = new SignedRequestHelper(_awskey, _awsSecretKey, _destination).Sign(requestparameters);
            XNamespace ns = _xmlnamespace;
            if (retrievalfunc == null)
                retrievalfunc = InvokeAmazonService;
            var xml = await retrievalfunc(url);
            var items = xml.Descendants(ns + "Item").Where((elem) => elem.Descendants(ns + "OfferListingId").Count() != 0)
                            .Select((e) => e.ToAmazonProduct(ns));

            return items;
        }

        private static AmazonShoppingCart ParseCartCreateAndCartAddXml(XNamespace ns, XDocument xml)
        {
            //check if errrors for this item...
            var amazonCartErrors = xml.Descendants(ns + "Cart")
                        .Where((elem) => elem.Descendants(ns + "Errors").Count() > 0)
                        .Select((e) =>
                            new AmazonShoppingCart
                            {
                                Error = string.Join(",", e.Descendants(ns + "Errors").Select(f => f.Value).ToList()),

                            }
                        );

            if (amazonCartErrors != null && amazonCartErrors.Count() > 0)
            {
                return amazonCartErrors.FirstOrDefault();
            }

            var cartDetails = from item in xml.Root.Descendants(ns + "Cart")
                              select new AmazonShoppingCart
                              {
                                  CartId = item.Element(ns + "CartId").Value,
                                  Hmac = item.Element(ns + "HMAC").Value,
                                  PurchaseUrl = item.Element(ns + "PurchaseURL").Value,
                                  AmazonProducts = item.Elements(ns + "CartItems").Elements(ns + "CartItem")
                                                    .Select(p => new AmazonProduct(p.Element(ns + "ASIN").Value)
                                                    {
                                                        Title = p.Element(ns + "Title").Value
                                                    }
                                                        ).ToList(),

                              };

            return cartDetails.FirstOrDefault();
        }

        private static AmazonShoppingCart ParseGetCartResponse(XNamespace ns, XDocument xml)
        {
            var cartDetails = from item in xml.Root.Descendants(ns + "Cart")
                              select new AmazonShoppingCart
                              {
                                  CartId = item.Element(ns + "CartId").Value,
                                  Hmac = item.Element(ns + "HMAC").Value,
                                  PurchaseUrl = item.Element(ns + "PurchaseURL").Value,
                                  AmazonProducts = item.Elements(ns + "CartItems").Elements(ns + "CartItem")
                                                   .Select(p => new AmazonProduct(p.Element(ns + "ASIN").Value)
                                                   {
                                                       Title = p.Element(ns + "Title").Value
                                                   }
                                                       ).ToList(),

                              };
            return cartDetails.FirstOrDefault();
        }

        public async Task<AmazonShoppingCart> AddToCart(string offerListingId, string asin, Func<string, Task<XDocument>> funcForAddToCart = null)
        {
            return await AddToCart(offerListingId, asin, null, null, funcForAddToCart);
        }

        private IDictionary<string, string> PrepareRequestParameters(string offerListingId, string cartId = null, string hmac = null)
        {
            if (string.IsNullOrEmpty(cartId))
            {
                requestparameters["Operation"] = "CartCreate";
                requestparameters["ResponseGroup"] = "";
                requestparameters.Remove("ResponseGroup");

            }
            else
            {
                requestparameters["Operation"] = "CartAdd";
                requestparameters["HMAC"] = hmac;
                requestparameters["CartId"] = cartId;
            }


            requestparameters["Item.1.OfferListingId"] = offerListingId;
            requestparameters["Item.1.Quantity"] = "1";

            return requestparameters;
        }

        public async Task<AmazonShoppingCart> AddToCart(string secondOfferlistingId, string asin, string cartId, string Hmac, Func<string, Task<XDocument>> funcForAddToCart = null)
        {
            XNamespace ns = _xmlnamespace;
            string url = string.Empty;
            requestparameters = PrepareRequestParameters(secondOfferlistingId, cartId, Hmac);

            url = new SignedRequestHelper(_awskey, _awsSecretKey, _destination).Sign(requestparameters);
            if (funcForAddToCart == null)
                funcForAddToCart = InvokeAmazonService;
            var xml = await funcForAddToCart(url);
            var Cart_CreateDetails = ParseCartCreateAndCartAddXml(ns, xml);
            return Cart_CreateDetails;
        }

        public async Task<AmazonShoppingCart> GetCart(string cartId, string hmac, Func<string, Task<XDocument>> funcForAddToCart = null)
        {
            XNamespace ns = _xmlnamespace;
            string url = string.Empty;
            requestparameters["ResponseGroup"] = "";
            requestparameters.Remove("ResponseGroup");
            requestparameters["Operation"] = "CartGet";
            requestparameters["HMAC"] = hmac;
            requestparameters["CartId"] = cartId;

            url = new SignedRequestHelper(_awskey, _awsSecretKey, _destination).Sign(requestparameters);
            if (funcForAddToCart == null)
                funcForAddToCart = InvokeAmazonService;
            var xml = await funcForAddToCart(url);
            var Cart_CreateDetails = ParseGetCartResponse(ns, xml);
            return Cart_CreateDetails;
        }

        public async Task<AmazonSearchResult> ItemSearch(string userInput, string responsegroup, int pageNumber = 0, Func<string, Task<XDocument>> retrievalfunc = null)
        {
            requestparameters["Operation"] = "ItemSearch";
            requestparameters["SearchIndex"] = "All";
            requestparameters["Keywords"] = userInput;
            requestparameters["ResponseGroup"] = responsegroup;
            if (pageNumber>0)
            {
                requestparameters["ItemPage"] = Convert.ToString(pageNumber);
            }

            string url = new SignedRequestHelper(_awskey, _awsSecretKey, _destination).Sign(requestparameters);
            System.Xml.Linq.XDocument xml = null;
            XNamespace ns = _xmlnamespace;
            if (retrievalfunc == null)
                retrievalfunc = InvokeAmazonService;

            xml = await retrievalfunc(url);
           
            var items = xml.Descendants(ns + "Item")
                .Where((elem) => elem.Descendants(ns + "OfferListingId").Count() != 0)
                .Select((e) => e.ToAmazonProduct(ns));

            var count = xml.Descendants(ns + "Items").Select(p => p.Element(ns + "TotalPages") == null ? string.Empty :
                                              p.Element(ns + "TotalPages").Value).FirstOrDefault();

            var amazonSearchItems = ConvertToAmazonSearch(items);
            amazonSearchItems.TotalPages = Convert.ToInt32(count);
            return amazonSearchItems;
          
        }

       
        private AmazonSearchResult ConvertToAmazonSearch(IEnumerable<AmazonProduct> products)
        {
            AmazonSearchResult amazonSearch = new AmazonSearchResult();
            amazonSearch.AmazonProducts = new List<AmazonProduct>();
            foreach (var item in products)
            {
                amazonSearch.AmazonProducts.Add(new AmazonProduct(item.ASIN)
                {
                    OfferListingId = item.OfferListingId,
                    SmallImage = item.SmallImage,
                    LargeImage = item.LargeImage,
                    MediumImage = item.MediumImage,
                    Features = item.Features,
                    FormattedPrice = item.FormattedPrice,
                    Title = item.Title
                });

            }
            return amazonSearch;
        }

    }
}
