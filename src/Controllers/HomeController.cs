using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Miniblog.Core.Models;
using Miniblog.Core.Services;

namespace src.Controllers
{
	public class HomeController : Controller
	{
		private readonly IBlogService _blog;
		public HomeController(IBlogService blog)
		{
			_blog = blog;
		}

		[Route("/")]
		[OutputCache(Profile = "default")]
		public async Task<IActionResult> Index()
		{
			var model = new HomeViewModel();
			var posts = await _blog.GetPosts(5);
			foreach (var post in posts)
			{
				model.LatestPosts.Add(new LatestPost()
				{
					Title = post.Title,
					Link = post.GetLink()
				});
			}
			return View(model);
		}
	}
}