using System.Linq;
using Microsoft.Extensions.Configuration;
using Miniblog.Core.Database;

namespace Miniblog.Core.Services
{
	public class DbBlogUserService : BaseBlogUserService, IUserServices
	{
		private readonly BlogContext _context;

		public DbBlogUserService(IConfiguration config, BlogContext context)
			: base(config)
		{
			_context = context;
		}

		public bool ValidateUser(string username, string password)
		{
			var user = _context.Users.SingleOrDefault(x => x.UserName == username);
			if (null != user)
			{
				return VerifyHashedPassword(password, user.Password, _config["user:salt"]);
			}
			return false;
		}
	}
}