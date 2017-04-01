using System.Text;

using TheBlueSky.SwiftOAuth.Services;

namespace TheBlueSky.SwiftOAuth.Test.Services
{
	internal sealed class FakeSigningAlgorithm : ISigningAlgorithm
	{
		public string SignatureMethod
		{
			get { return nameof(FakeSigningAlgorithm); }
		}

		public byte[] ComputeHash(byte[] buffer, byte[] key)
		{
			return Encoding.UTF8.GetBytes(this.SignatureMethod);
		}
	}
}
