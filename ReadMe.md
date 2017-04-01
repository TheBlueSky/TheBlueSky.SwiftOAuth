# OAuth 1.0a Client Library for .NET #

## What is it? ##

An OAuth 1.0a client library for .NET. Its aim is to provide the clients with a simple and straightforward API in order to authenticate against OAuth 1.0a flow and consume protected resources.

## How to get it? ##

To install `TheBlueSky.SwiftOAuth`, run the following command in the Package Manager Console:

`Install-Package TheBlueSky.SwiftOAuth`

Or search for `TheBlueSky.SwiftOAuth` in NuGet Package Manager.

## Usage ##

This section assumes you are familiar with OAuth 1.0a protocol; if you're not, feel free to read about it before you continue.

1. Get your Consumer Key and Consumer Secret from the service provider, and choose your Redirect URL.
2. Get the Signing Algorithm that is supported by the service provider.
3. Reference `TheBlueSky.SwiftOAuth` library.
4. Create a class that implements `TheBlueSky.SwiftOAuth.Services.ISigningAlgorithm` interface to implement the service provider Signing Algorithm; for example:

```csharp
internal sealed class MyHMACSHA1 : ISigningAlgorithm
{
    public string SignatureMethod
    {
        get { return "HMAC-SHA1"; }
    }

    public byte[] ComputeHash(byte[] buffer, byte[] key)
    {
        var algorithm = new System.Security.Cryptography.HMACSHA1 { Key = key };
        return algorithm.ComputeHash(buffer);
    }
}
```

5. Instantiate `OAuth10`; for example:

```csharp
var oauth = new OAuth10(ConsumerKey, ConsumerSecret, RedirectUrl, new MyHMACSHA1());
```

6. Request a Request Token; for example:

```csharp
var token = await oauth.GetRequestTokenAsync(new Uri($"{ServiceProviderUrl}/oauth/request_token"), HttpMethod.Post);
```

7. Use the Request Token to send the user to the service provider log in page; for example:

```csharp
var authorizeUrl = $"{ServiceProviderUrl}/oauth/authorize?oauth_token={token.Token}&oauth_callback={RedirectUrl}";
Process.Start(authorizeUrl);
```

8. The user will log in. If the user logs in successfully and provided their consent to your app, the service provider will send the user to your Redirect URL where you can get the tokens generated after this step; let's call them `authenticationResult`.

9. Request an Access Token using `authenticationResult`; for example:

```csharp
var token = await oauth.GetAccessTokenAsync(new Uri($"{ServiceProviderUrl}/oauth/access_token"), HttpMethod.Post, authenticationResult);
```

10. Now you have all you need to access a protected resource. To do so, sign the protected resource URL before calling it; for example:

```csharp
var url = oauth.GetOAuthSignedUrl(new Uri($"{ServiceProviderUrl}/resource?param1={param1}&param2={param2}"), HttpMethod.Get);
var result = await httpClient.GetAsync(url);
```

OAuth 1.0a isn't the simplest flow. This library relieves from writing the boilerplate code necessary to comply with the protocol specifications, in order to successfully authenticate your client and authorise them to access the protected resources. 

## Supported Frameworks ##

* .NET Framework 4.5.1
* WinRT for Windows 8.1 and Windows Phone 8.1
* Xamarin.Android
* Xamarin.iOS
* Xamarin.Mac

And it can also be referenced from libraries that target .NET Standard 1.2.
