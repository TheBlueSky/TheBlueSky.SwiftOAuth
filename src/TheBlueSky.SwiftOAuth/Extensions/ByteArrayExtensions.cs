using System;

namespace TheBlueSky.SwiftOAuth.Extensions
{
	public static class ByteArrayExtensions
	{
		public static string ToBase64(this byte[] bytes)
		{
			return Convert.ToBase64String(bytes);
		}
	}
}
