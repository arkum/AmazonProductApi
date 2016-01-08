using AmazonApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AmazonApi
{
    public interface IAmazonProductApi
    {
        //IEnumerable<AmazonProduct> ItemLookup(string[] productides, string url);


        Task<IEnumerable<AmazonProduct>> ItemLookup(string[] productids, string responsegroup, Func<string, Task<XDocument>> retrievalfunc = null);
        //Task<AmazonShoppingCart> AddToCart(string id, Func<string, Task<XDocument>> retrievalfunc = null);
        //Task<AmazonShoppingCart> AddToCart(string productId, string cartId, string hmac, Func<string, Task<XDocument>> retrievalfunc = null);

        Task<AmazonProduct> ItemDetails(string ASIN, Func<string, Task<XDocument>> retrievalfunc = null);

        Task<IEnumerable<AmazonProduct>> SimilarItems(string ASIN, Func<string, Task<XDocument>> retrievalfunc = null);

        Task<AmazonShoppingCart> GetCart(string cartId, string hmac, Func<string, Task<XDocument>> retrievalfunc = null);

        Task<AmazonShoppingCart> AddToCart(string offerlistingid, string asin, Func<string, Task<XDocument>> retrievalfunc = null);

        Task<AmazonShoppingCart> AddToCart(string secondOfferlistingId, string asin, string cartId, string Hmac, Func<string, Task<XDocument>> funcForAddToCart = null);

        Task<AmazonSearchResult> ItemSearch(string userInput,string responsegroup, int pageNumber = 0, Func<string, Task<XDocument>> retrievalfunc = null);

      
    }
}
