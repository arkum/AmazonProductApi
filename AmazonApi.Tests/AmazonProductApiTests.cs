using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;
using System.Diagnostics;
using AmazonProductAdvtApi;
using System.Net;
using AmazonApi;
using AmazonApi.Models;

namespace AmazonApi.Tests
{
    public class AmazonProductApiTests
    {
        const string awskey = "<your aws key>";
        const string awsSecretKey = "<your aws secret key>";
        const string destination = "webservices.amazon.com";
        const string associateTag = "<your associate tag>";
        const string apiVersion = "2013-08-01";
        const string xmlnamespace = "http://webservices.amazon.com/AWSECommerceService/2013-08-01";
        IAmazonProductApi amazonapi;
        string[] productids;
        Func<string, Task<XDocument>> retrievelfunc;
        Func<string, Task<XDocument>> funcForAddToCart;
        Func<string, Task<XDocument>> funcForCreteCart;
        Func<string, Task<XDocument>> funcForGetCart;
        Func<string, Task<XDocument>> funcForItemSearch;
        public AmazonProductApiTests()
        {
            var awsconfiguration = new AwsConfiguration(awskey, awsSecretKey, destination, apiVersion, associateTag, xmlnamespace);
            amazonapi = new AmazonProductApi(awsconfiguration);
            productids = new string[] { "B004YLCJXC", "B006GOSIYY", "B00KHR4T8U", "B00MHIWT8O", "B00NH2VLGK" };
            retrievelfunc = (url) => Task.FromResult(XDocument.Load(@"Assets\amazon.xml"));
            funcForAddToCart = (url) => Task.FromResult(XDocument.Load(@"Assets\addtocart.xml"));
            funcForCreteCart = (url) => Task.FromResult(XDocument.Load(@"Assets\createcart.xml"));
            funcForGetCart = (url) => Task.FromResult(XDocument.Load(@"Assets\getcart.xml"));
            funcForItemSearch = (url) => Task.FromResult(XDocument.Load(@"Assets\ItemSearch.xml"));
            AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory);

        }
        

        [Fact]
        public async void ItemLookupCountTest()
        {
            var products = await amazonapi.ItemLookup(productids, "Images,ItemAttributes,Offers", retrievelfunc);
            Assert.Equal(products.Count(), 2);
        }
        [Fact]
        public async void ItemLookreturnsItemsIaskedFor()
        {
           //var productids = new string[] { "B004YLCJXC", "B006GOSIYY", "B00KHR4T8U", "B00MHIWT8O", "B00NH2VLGK" };
            var products = await amazonapi.ItemLookup(productids, "Images,ItemAttributes,Offers", retrievelfunc);
            Assert.Contains(products, p => productids.Contains(p.ASIN));
        }
        [Fact]
        public async void ItemLookupShouldReturnOnlyProductsWhichhasprice()
        {
            var products = await amazonapi.ItemLookup(productids, "Images,ItemAttributes,Offers", retrievelfunc);
            Assert.Contains(products, p => !string.IsNullOrEmpty(p.FormattedPrice));

        }
        [Fact]
        public async void ItemLookupShouldReturnOnlyProductsWhichhassmallimage()
        {
            var products = await amazonapi.ItemLookup(productids, "Images,ItemAttributes,Offers", retrievelfunc);
            Assert.Contains(products, p => !string.IsNullOrEmpty(p.SmallImage));

        }
        [Fact]
        public async void ItemLookupShouldReturnOnlyProductsWhichhasmediumimage()
        {
            var products = await amazonapi.ItemLookup(productids, "Images,ItemAttributes,Offers", retrievelfunc);
            Assert.Contains(products, p => !string.IsNullOrEmpty(p.MediumImage));

        }
        [Fact]
        public async void ItemLookupShouldReturnOnlyProductsWhichhaslargeimage()
        {
            var products = await amazonapi.ItemLookup(productids, "Images,ItemAttributes,Offers", retrievelfunc);
            Assert.Contains(products, p => !string.IsNullOrEmpty(p.LargeImage));

        }
        [Fact]
        public async void ItemLookupFeaturessdhouldnotbeempty()
        {
            var products = await amazonapi.ItemLookup(productids, "Images,ItemAttributes,Offers", retrievelfunc);
            Assert.Contains(products, p => p.Features.Count() > 0);
        }

        /// <summary>
        /// 1. Arrange -> Arragne two offerlistingids 
        /// 2. Add the offerlisting ids to Amazon
        /// 3. Assert -> check the purchase url has Associate tag.
        /// </summary>
        [Fact]
        public async void AddToCartUrlShouldContainAssociateTag()
        {
            string asin = "B00MHIWT8O";
            string offerlistingid = "GT5MOtJKMw77jOdMW4BTQj5%2B7tzBJuDfhgPgizmTlVpta1FvMD5GdoAedgwNH%2FFLrD1ZTXIliEGoE0VilYpxIUpnga3C%2BgE7ak6ILebxaKlu6Dl8Ad5%2BGNwqV8dM9uHbNqzTCj7n6C8c%2F13NocmgVdqxvviGmfNh";
            funcForAddToCart = (url) => Task.FromResult(XDocument.Load(@"Assets\cartcreate.xml"));
            var awsconfigurationforcartCreate = new AwsConfiguration(awskey, awsSecretKey, destination, apiVersion, associateTag, "http://webservices.amazon.com/AWSECommerceService/2011-08-01");
            var amazonapiforcartCreate = new AmazonProductApi(awsconfigurationforcartCreate);
            AmazonShoppingCart shoppingCart = await amazonapiforcartCreate.AddToCart(offerlistingid, asin, funcForAddToCart);
            Assert.Contains(associateTag, shoppingCart.PurchaseUrl);
        }

        /// <summary>
        /// 1. Arrange -> Arragne two offerlistingids 
        /// 2. Add the offerlisting ids to Amazon
        /// 3. add the second item offerlisting id to the amazon 
        /// 4. Assert -> check the purchase url has Associate tag.
        /// </summary>
        [Fact]
        public async void AddToCartUrlWithMultipleItemsShouldContainAssociateTag()
        {
            string asin = "B00MHIWT8O";
            string offerlistingid = "GT5MOtJKMw77jOdMW4BTQj5%2B7tzBJuDfhgPgizmTlVpta1FvMD5GdoAedgwNH%2FFLrD1ZTXIliEGoE0VilYpxIUpnga3C%2BgE7ak6ILebxaKlu6Dl8Ad5%2BGNwqV8dM9uHbNqzTCj7n6C8c%2F13NocmgVdqxvviGmfNh";
            funcForAddToCart = (url) => Task.FromResult(XDocument.Load(@"Assets\cartcreate.xml"));
            var awsconfigurationforcartCreate = new AwsConfiguration(awskey, awsSecretKey, destination, apiVersion, associateTag, "http://webservices.amazon.com/AWSECommerceService/2011-08-01");
            var amazonapiforcartCreate = new AmazonProductApi(awsconfigurationforcartCreate);
            AmazonShoppingCart shoppingCart = await amazonapiforcartCreate.AddToCart(offerlistingid, asin, funcForAddToCart);

            // add multiple items..
            string secondOfferlistingId = "zd2pVJTM5AmYa6u1atwuQvHljDgDEozIAObG38viMz21SO5PNOSFB4Y4uke%2BRImUem3L2A6uciG6%2F2EjYgnufv0eZv%2BVcQ2FRL54S8LHhwitXUkG4ZFMgA%3D%3D";
            asin = "B00ZQ4JQAA";
            funcForAddToCart = (url) => Task.FromResult(XDocument.Load(@"Assets\addtocartmultipleitem.xml"));
            AmazonShoppingCart shoppingCartItems = await amazonapiforcartCreate.AddToCart(secondOfferlistingId, asin, shoppingCart.CartId, shoppingCart.Hmac, funcForAddToCart);

            ////get the cart details with getcart operations...
            //var getAmazonCart = await amazonapi.GetCart(amazonShoppingCart[0].CartId, amazonShoppingCart[0].Hmac);
            Assert.Contains(associateTag, shoppingCartItems.PurchaseUrl);
        }

        [Fact]
        public async void GetCartShouldNotBeEmptyOrNull()
        {
            string asin = "B00MHIWT8O";
            string offerlistingid = "GT5MOtJKMw77jOdMW4BTQj5%2B7tzBJuDfhgPgizmTlVpta1FvMD5GdoAedgwNH%2FFLrD1ZTXIliEGoE0VilYpxIUpnga3C%2BgE7ak6ILebxaKlu6Dl8Ad5%2BGNwqV8dM9uHbNqzTCj7n6C8c%2F13NocmgVdqxvviGmfNh";
            funcForAddToCart = (url) => Task.FromResult(XDocument.Load(@"Assets\cartcreate.xml"));
            var awsconfigurationforcartCreate = new AwsConfiguration(awskey, awsSecretKey, destination, apiVersion, associateTag, "http://webservices.amazon.com/AWSECommerceService/2011-08-01");
            var amazonapiforcartCreate = new AmazonProductApi(awsconfigurationforcartCreate);
            AmazonShoppingCart shoppingCart = await amazonapiforcartCreate.AddToCart(offerlistingid, asin, funcForAddToCart);

            // add multiple items..
            string secondOfferlistingId = "zd2pVJTM5AmYa6u1atwuQvHljDgDEozIAObG38viMz21SO5PNOSFB4Y4uke%2BRImUem3L2A6uciG6%2F2EjYgnufv0eZv%2BVcQ2FRL54S8LHhwitXUkG4ZFMgA%3D%3D";
            asin = "B00ZQ4JQAA";
            funcForAddToCart = (url) => Task.FromResult(XDocument.Load(@"Assets\addtocartmultipleitem.xml"));
            AmazonShoppingCart shoppingCartItems = await amazonapiforcartCreate.AddToCart(secondOfferlistingId, asin, shoppingCart.CartId, shoppingCart.Hmac, funcForAddToCart);

            ////get the cart details with getcart operations...
            var getCart = await amazonapi.GetCart(shoppingCart.CartId, shoppingCart.Hmac);
            Assert.Contains(associateTag, getCart.PurchaseUrl);
        }
        /// <summary>
        /// 1.Set the  Operation as Itemsearch All
        /// 2. SearchIndex as All 
        /// 3. set the Keyword as any value from user input..
        /// </summary>
        [Fact]
        public async void ItemSearchShouldNotBeEmptyOrNull()
        {
              string userInput = "asus";
              funcForItemSearch = (url) => Task.FromResult(XDocument.Load(@"Assets\ItemSearch.xml"));
              //var awsconfig = new AwsConfiguration(awskey, awsSecretKey, destination, apiVersion, associateTag, "http://webservices.amazon.com/AWSECommerceService/2011-08-01");
              //var amazonApiSearchItem = new AmazonProductApi(awsconfig);
              var searchItemList = await amazonapi.ItemSearch(userInput,"Images,ItemAttributes,Offers");

              Assert.NotNull(searchItemList.AmazonProducts);
              Assert.True(searchItemList.TotalPages > 0);
        }

        /// <summary>
        /// 1.Set the  Operation as Itemsearch with page numbers
        /// 2. SearchIndex as All 
        /// 3. set the Keyword as any value from user input..
        /// 4. set the ItemPage page attribute to test...
        /// 5. Assert with both the list should have different Asin ids..
        /// </summary>
        [Fact]
        public async void ItemSearchWithPageNumberShouldNotBeEmptyOrNull()
        {
            string userInput = "asus";
            funcForItemSearch = (url) => Task.FromResult(XDocument.Load(@"Assets\ItemSearch.xml"));
            var searchItemList = await amazonapi.ItemSearch(userInput,"Images,ItemAttributes,Offers");

            //Check with page numbers..Need to check with Arun..how to pass the page numbers..
            var searchItemWithpage4 = await amazonapi.ItemSearch(userInput,"Images,ItemAttributes,Offers",4);

            Assert.True(searchItemList.TotalPages > 0);
            Assert.NotNull(searchItemList.AmazonProducts);
            //page numbers asserts 
            Assert.True(searchItemWithpage4.TotalPages > 0);
            //both the list have different asin numbers..
            Assert.NotEqual(searchItemWithpage4.AmazonProducts[0].ASIN, searchItemList.AmazonProducts[0].ASIN);
        }

        

       
        private static List<AmazonShoppingCart> ConvertToShoppingCart(IEnumerable<AmazonProduct> products)
        {
            var shoppingCart = (from p in products
                                select new AmazonShoppingCart { OfferListingId = p.OfferListingId, Asin = p.ASIN }).ToList();
            return shoppingCart;
        }
    }
}
