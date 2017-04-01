namespace TheBlueSky.SwiftOAuth.Models
{
	public class OAuthToken
	{
		public string Token { get; internal set; }

		public string Secret { get; internal set; }

		public string Verifier { get; internal set; }
	}
}
