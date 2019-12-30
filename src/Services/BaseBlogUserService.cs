using System;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;

namespace Miniblog.Core.Services
{
	public class BaseBlogUserService
	{
		protected readonly IConfiguration _config;

		public BaseBlogUserService(IConfiguration config)
		{
			_config = config;
		}

		protected bool VerifyHashedPassword(string passwordSupplied, string hasedPassword, string salt)
		{
			byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

			byte[] hashBytes = KeyDerivation.Pbkdf2(
				password: passwordSupplied,
				salt: saltBytes,
				prf: KeyDerivationPrf.HMACSHA1,
				iterationCount: 1000,
				numBytesRequested: 256 / 8
			);

			string hashText = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
			return hashText == hasedPassword;
		}

	}
}