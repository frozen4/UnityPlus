using System;
using System.Security.Cryptography;

// ReSharper disable once CheckNamespace
namespace Common
{
	public class RandomGenerator : Random
	{
		public RandomGenerator() : base(GenerateSeed())
		{
		}

		public static int GenerateSeed()
		{
			var rng = new byte[sizeof(int)];
			var randomNumberGenerator = RandomNumberGenerator.Create();
			randomNumberGenerator.GetBytes(rng);
			var seed = BitConverter.ToInt32(rng, 0);
			return seed;
		}
	}
}
