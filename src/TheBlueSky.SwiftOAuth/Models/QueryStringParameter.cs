using TheBlueSky.SwiftOAuth.Extensions;

namespace TheBlueSky.SwiftOAuth.Models
{
	internal class QueryStringParameter
	{
		public string Key { get; set; }

		public string Value { get; set; }

		public override string ToString()
		{
			return $"{this.Key}={this.Value.EscapeData()}";
		}
	}
}
