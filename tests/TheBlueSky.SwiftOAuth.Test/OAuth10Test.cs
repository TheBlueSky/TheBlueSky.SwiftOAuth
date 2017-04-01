using System;
using System.Linq;
using System.Net.Http;

using Xunit;

using TheBlueSky.SwiftOAuth.Models;
using TheBlueSky.SwiftOAuth.Test.Services;

namespace TheBlueSky.SwiftOAuth.Test
{
	public class OAuth10Test
	{
		private const string ConsumerKey = "afM5aDwTtbucpkMM3mgq7wRHmv975M6HBv6THqkx";
		private const string ConsumerSecret = "Y7wUNJENmmdYQXKjQXdQZPYFxqDLS9fhw3quGxgD";
		private const string CallbackUrl = "https://www.google.com";
		private const string Token = "WMAtTZjS7atxwKU46kXt7YUHRZcUBxXkqhCGZ9MJ";
		private const string Secret = "FpYgGPNZFNLxU8t4wCLjQghARU3zTvSyuNNtLfqJ";
		private const string Verifier = "QLych8krhuHyq5vJ6x5GdYY6cJpWv3ayMvWFG5Vs";

		private static readonly FakeSigningAlgorithm SigningAlgorithm = new FakeSigningAlgorithm();

		public class GetOAuthSignedUrlTest
		{
			[Fact]
			public void UrlIsSignedUsingTheDefinedAlgorithm()
			{
				var url = new Uri("https://www.google.com/#q=oauth+1.0a");
				var signature = Uri.EscapeDataString("RmFrZVNpZ25pbmdBbGdvcml0aG0=");

				var oauth = new OAuth10(ConsumerKey, ConsumerSecret, CallbackUrl, SigningAlgorithm);
				oauth.UseExistingToken(Token, Secret, Verifier);

				var oauthSignedUrl = oauth.GetOAuthSignedUrl(url, HttpMethod.Post);
				var oauthSignature = oauthSignedUrl.Split(new[] { '&' })
					.FirstOrDefault(s => s.StartsWith("oauth_signature="))
					?.Split(new[] { '=' })[1];

				Assert.Equal(signature, oauthSignature);
			}
		}

		public class UseExistingTokenTest
		{
			[Fact]
			public void TokenIsSetCorrectlyUsingTheProvidedValues()
			{
				var token = new OAuthToken { Token = Token, Secret = Secret, Verifier = Verifier };

				var oauth = new OAuth10(ConsumerKey, ConsumerSecret, CallbackUrl, SigningAlgorithm);
				oauth.UseExistingToken(Token, Secret, Verifier);

				var oauthToken = oauth.Token;

				var isEqual = token.Token == oauthToken.Token &&
					token.Secret == oauthToken.Secret &&
					token.Verifier == oauthToken.Verifier;

				Assert.True(isEqual);
			}
		}
	}
}
