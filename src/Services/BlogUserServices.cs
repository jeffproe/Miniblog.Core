using Microsoft.Extensions.Configuration;

namespace Miniblog.Core.Services
{
	public class BlogUserServices : BaseBlogUserService, IUserServices
	{
		public BlogUserServices(IConfiguration config)
			: base(config)
		{ }

		public bool ValidateUser(string username, string password)
		{
			return username == _config["user:username"] && VerifyHashedPassword(password, _config["user:password"], _config["user:salt"]);
		}
	}
}
