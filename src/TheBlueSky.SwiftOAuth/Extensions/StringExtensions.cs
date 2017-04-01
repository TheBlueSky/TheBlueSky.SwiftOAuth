using System;
using System.Text;

namespace TheBlueSky.SwiftOAuth.Extensions
{
	internal static class StringExtensions
	{
		public static string EscapeData(this string @string)
		{
			return Uri.EscapeDataString(@string);
		}

		public static byte[] GetBytes(this string @string)
		{
			return Encoding.UTF8.GetBytes(@string);
		}
	}
}
