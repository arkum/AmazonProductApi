using AmazonApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace AmazonApi
{
    public static class Extensions
    {
        public static AmazonProduct ToAmazonProduct(this XElement xElement, XNamespace ns)
        {
            return new AmazonProduct(xElement.Element(ns + "ASIN").Value)
            {
                Title = xElement.GetTitle(ns),
                FormattedPrice = xElement.GetFormattedPrice(ns),
                Features = xElement.GetFatures(ns),
                SmallImage = xElement.GetSmallImage(ns),
                MediumImage = xElement.GetMediumImage(ns),
                LargeImage = xElement.GetLargeImage(ns),
                OfferListingId = xElement.GetOfferListingId(ns),
                ImageSets = xElement.GetImageSet(ns),
                Description = xElement.GetDescription(ns),
                Availability = xElement.GetAvailability(ns)
            };
        }
        public static string ElementValue(this XElement xElement, string elementTag, XNamespace ns)
        {
            return xElement == null ? string.Empty : xElement.Element(ns + elementTag) == null ? string.Empty : xElement.Element(ns + elementTag).Value;
        }
        public static string GetTitle(this XElement xElement, XNamespace ns)
        {
            return xElement.Element(ns + "ItemAttributes") == null ? string.Empty : xElement.Element(ns + "ItemAttributes").ElementValue("Title", ns);
        }
        public static string GetFormattedPrice(this XElement xElement, XNamespace ns)
        {
            return xElement.Element(ns + "OfferSummary") == null ? string.Empty : xElement.Element(ns + "OfferSummary").Element(ns + "LowestNewPrice") == null ? string.Empty : xElement.Element(ns + "OfferSummary").Element(ns + "LowestNewPrice").ElementValue("FormattedPrice", ns);
        }
        public static string[] GetFatures(this XElement xElement,XNamespace ns)
        {
            return xElement.Descendants(ns + "Feature") == null ? new string[] {}: xElement.Descendants(ns + "Feature").Select(f => f.Value).ToArray();
        }
        public static string GetSmallImage(this XElement xElement, XNamespace ns)
        {
            return xElement.Element(ns + "SmallImage") == null ? string.Empty : xElement.Element(ns + "SmallImage").ElementValue("URL", ns);
        }
        public static string GetMediumImage(this XElement xElement, XNamespace ns)
        {
            return xElement.Element(ns + "MediumImage") == null ? string.Empty : xElement.Element(ns + "MediumImage").ElementValue("URL", ns);
        }
        public static string GetLargeImage(this XElement xElement, XNamespace ns)
        {
            return xElement.Element(ns + "LargeImage") == null ? string.Empty : xElement.Element(ns + "LargeImage").ElementValue("URL", ns);
        }
        public static string GetOfferListingId(this XElement xElement, XNamespace ns)
        {
            return xElement.Element(ns + "Offers") == null? string.Empty : xElement.Element(ns + "Offers").Elements(ns + "Offer") == null ? string.Empty :
                    xElement.Element(ns + "Offers").Elements(ns + "Offer").Elements(ns + "OfferListing") == null ? string.Empty :
                    xElement.Element(ns + "Offers").Elements(ns + "Offer").Elements(ns + "OfferListing").Descendants(ns + "OfferListingId") == null ? string.Empty : 
                    xElement.Element(ns + "Offers").Elements(ns + "Offer").Elements(ns + "OfferListing").Descendants(ns + "OfferListingId").Select(f => f == null ? string.Empty : f.Value).First().ToString();
        }
        public static IEnumerable<ProductImage> GetImageSet(this XElement xElement, XNamespace ns)
        {
            return xElement.Descendants(ns + "ImageSet") == null ? new ProductImage[]{} :
                    xElement.Descendants(ns + "ImageSet").Select(im => new ProductImage
                    {
                        MediumImage = im.Element(ns + "MediumImage") == null ? string.Empty : im.Element(ns + "MediumImage").ElementValue("URL", ns),
                        LargeImage = im.Element(ns + "LargeImage") == null ? string.Empty : im.Element(ns + "LargeImage").ElementValue("URL", ns)
                    });
        }
        public static string GetDescription(this XElement xElement, XNamespace ns)
        {
            return xElement.Element(ns + "EditorialReviews") == null ? string.Empty : xElement.Element(ns + "EditorialReviews").Element(ns + "EditorialReview") == null ? string.Empty : xElement.Element(ns + "EditorialReviews").Element(ns + "EditorialReview").ElementValue("Content", ns);
        }
        public static string GetAvailability(this XElement xElement, XNamespace ns)
        {
            return xElement.Element(ns + "Offers") == null ? string.Empty :
                xElement.Element(ns + "Offers").Element(ns + "Offer") == null ? string.Empty :
                xElement.Element(ns + "Offers").Element(ns + "Offer").Element(ns + "OfferListing") == null ? string.Empty :
                xElement.Element(ns + "Offers").Element(ns + "Offer").Element(ns + "OfferListing").ElementValue("Availability", ns);
        }

    }
}