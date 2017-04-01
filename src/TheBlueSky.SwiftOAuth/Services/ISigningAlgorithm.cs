namespace TheBlueSky.SwiftOAuth.Services
{
	public interface ISigningAlgorithm
	{
		string SignatureMethod { get; }

		byte[] ComputeHash(byte[] buffer, byte[] key);
	}
}
