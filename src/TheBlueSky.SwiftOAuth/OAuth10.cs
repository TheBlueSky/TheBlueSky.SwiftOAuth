using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using TheBlueSky.SwiftOAuth.Extensions;
using TheBlueSky.SwiftOAuth.Models;
using TheBlueSky.SwiftOAuth.Services;

namespace TheBlueSky.SwiftOAuth
{
	public sealed class OAuth10
	{
		private const int MinRandomNumber = 1234567;
		private const int MaxRandomNumber = 999999999;
		private const string OAuthVersion = "1.0";

		private static readonly HttpClient RequestClient;
		private static readonly Random RandomGenerator = new Random();

		private readonly string consumerKey;
		private readonly string consumerSecret;
		private readonly string callbackUrl;
		private readonly ISigningAlgorithm signingAlgorithm;

		static OAuth10()
		{
			RequestClient = new HttpClient();
		}

		public OAuth10(string consumerKey, string consumerSecret, string callbackUrl, ISigningAlgorithm signingAlgorithm)
		{
			this.consumerKey = consumerKey;
			this.consumerSecret = consumerSecret;
			this.callbackUrl = callbackUrl;
			this.signingAlgorithm = signingAlgorithm;
		}

		public OAuthToken Token { get; private set; }

		public async Task<OAuthToken> GetAccessTokenAsync(Uri url, HttpMethod httpMethod, string authenticationResult)
		{
			var parameters = ParseQueryString(authenticationResult);
			this.Token.Verifier = parameters.Where(p => p.Key == "oauth_verifier").FirstOrDefault()?.Value;

			await RequestToken(url, httpMethod).ConfigureAwait(false);

			this.Token.Verifier = null;

			return this.Token;
		}

		public string GetOAuthSignedUrl(Uri url, HttpMethod httpMethod)
		{
			var oauthParameters = this.GetOAuthParameters();
			var requestParameters = ParseQueryString(url.Query);
			requestParameters.AddRange(oauthParameters);

			var sortedParameters = requestParameters.OrderBy(p => p.Key).ThenBy(p => p.Value).ToList();
			var normalisedUrl = GetOAuthNormalisedUrl(url);
			var signature = this.GetOAuthSignature(normalisedUrl, sortedParameters, httpMethod);

			sortedParameters.Add(new QueryStringParameter { Key = "oauth_signature", Value = signature });

			return $"{normalisedUrl}?{string.Join("&", sortedParameters)}";
		}

		public async Task<OAuthToken> GetRequestTokenAsync(Uri url, HttpMethod httpMethod)
		{
			this.Token = new OAuthToken();

			await RequestToken(url, httpMethod).ConfigureAwait(false);

			return this.Token;
		}

		public void UseExistingToken(string token, string secret, string verifier)
		{
			this.Token = new OAuthToken {
				Token = token,
				Secret = secret,
				Verifier = verifier
			};
		}

		private static string GetOAuthNormalisedUrl(Uri url)
		{
			var normalisedUrlBuilder = new StringBuilder($"{url.Scheme}://{url.Host}");
			normalisedUrlBuilder.Append($"{(url.IsDefaultPort ? string.Empty : $":{url.Port}")}");
			normalisedUrlBuilder.Append(url.AbsolutePath);

			return normalisedUrlBuilder.ToString();
		}

		private List<QueryStringParameter> GetOAuthParameters()
		{
			var oauthParameters = new List<QueryStringParameter> {
				new QueryStringParameter { Key = "oauth_callback", Value = this.callbackUrl },
				new QueryStringParameter { Key = "oauth_consumer_key", Value = this.consumerKey },
				new QueryStringParameter { Key = "oauth_nonce", Value = $"{RandomGenerator.Next(MinRandomNumber, MaxRandomNumber)}" },
				new QueryStringParameter { Key = "oauth_signature_method", Value = this.signingAlgorithm.SignatureMethod },
				new QueryStringParameter { Key = "oauth_timestamp", Value = $"{GetTimestamp()}" },
				new QueryStringParameter { Key = "oauth_version", Value = OAuthVersion },
			};

			if (!string.IsNullOrEmpty(this.Token?.Token))
			{
				oauthParameters.Add(new QueryStringParameter { Key = "oauth_token", Value = this.Token.Token });
			}

			if (!string.IsNullOrEmpty(this.Token?.Verifier))
			{
				oauthParameters.Add(new QueryStringParameter { Key = "oauth_verifier", Value = this.Token.Verifier });
			}

			return oauthParameters;
		}

		private string GetOAuthSignature(string url, List<QueryStringParameter> requestParameters, HttpMethod httpMethod)
		{
			var baseString = $"{httpMethod}&{url.EscapeData()}&{string.Join("&", requestParameters).EscapeData()}";
			var signatureKey = $"{consumerSecret}&{this.Token?.Secret ?? string.Empty}".GetBytes();
			var baseStringHash = signingAlgorithm.ComputeHash(baseString.GetBytes(), signatureKey);

			return baseStringHash.ToBase64();
		}

		private static long GetTimestamp()
		{
			return Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
		}

		private static List<QueryStringParameter> ParseQueryString(string query)
		{
			var httpRequestParameters = new List<QueryStringParameter>();

			// e.g., oauth_token=zgXhEVx89paKu6Sa6KecTFp9GLLhDJhrwLTaVGCY&oauth_token_secret=pYsqTQi0PpmOkid7gsTLLhnE51La8wQ4S7Vqwdwd&oauth_callback_confirmed=true
			query = query.TrimStart(new [] { '?' });

			if (!string.IsNullOrEmpty(query))
			{
				var parameters = query.Split(new [] { '&' });

				foreach (var parameter in parameters)
				{
					var keyValuePairs = parameter.Split(new [] { '=' });
					httpRequestParameters.Add(new QueryStringParameter { Key = keyValuePairs[0], Value = keyValuePairs.Length > 1 ? keyValuePairs[1] : string.Empty });
				}
			}

			return httpRequestParameters;
		}

		private async Task RequestToken(Uri url, HttpMethod httpMethod)
		{
			if (httpMethod != HttpMethod.Get && httpMethod != HttpMethod.Post)
			{
				throw new NotSupportedException("Only 'GET' and 'POST' HTTP Methods are supported.");
			}

			var signedUrl = this.GetOAuthSignedUrl(url, httpMethod);

			var response = httpMethod == HttpMethod.Get ?
				await RequestClient.GetAsync(signedUrl).ConfigureAwait(false) :
				await RequestClient.PostAsync(signedUrl, null).ConfigureAwait(false);

			if (response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				var parameters = ParseQueryString(content);

				this.Token.Token = parameters.Where(p => p.Key == "oauth_token").FirstOrDefault()?.Value;
				this.Token.Secret = parameters.Where(p => p.Key == "oauth_token_secret").FirstOrDefault()?.Value;
			}
		}
	}
}
