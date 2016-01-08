# AmazonProductApi
Amazon Product API wrapper in C#

```cs
  var awsconfiguration = new AwsConfiguration(awskey, awsSecretKey, destination, apiVersion, associateTag, xmlnamespace);
  var amazonapi = new AmazonProductApi(awsconfiguration);
  var products = await amazonapi.ItemLookup(productids, "Images,ItemAttributes,Offers");
            
```
You can optional pass a ``` Func<string, Task<XDocument>> ``` to avoid hitting amazon services in unit tests

```cs
var products = await amazonapi.ItemLookup(productids, "Images,ItemAttributes,Offers", retrievelfunc);
```

Uses signed request helper from here https://aws.amazon.com/code/Product-Advertising-API/2480
