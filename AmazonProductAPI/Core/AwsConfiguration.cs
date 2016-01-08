using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AmazonApi
{
    public class AwsConfiguration : Tuple<string, string, string, string, string, string>
    {
        //public AwsConfiguration() : base(Configuration.AWS_KEY, Configuration.AWS_SECRET_KEY, Configuration.Destination, Configuration.ApiVersion, Configuration.AssociateTag, Configuration.XmlNamesapce)
        //{

        //}
        public AwsConfiguration(string awskey, string awsSecretKey, string destination, string apiVersion, string associateTag, string xmlnamespace)
            : base(awskey, awsSecretKey,destination,apiVersion, associateTag, xmlnamespace)
        {

        }
        public string AwsKey { get {return Item1 ;} }
        public string AwsSecretKey { get {return Item2 ;}  }
        public string Destination { get { return Item3; }  }
        public string ApiVersion { get { return Item4; }  }
        public string AssociateTag { get { return Item5; } }
        public string XmlNamespace{ get { return Item6; } }

    }
}