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
			var model = await GetPageWithLatestAsync("home");
			return View(model);
		}

		[Route("/about")]
		[OutputCache(Profile = "default")]
		public async Task<IActionResult> About()
		{
			var model = await GetPageWithLatestAsync("about");
			return View(model);
		}

		private async Task<HomeViewModel> GetPageWithLatestAsync(string slug)
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

			model.Post = await _blog.GetPostBySlug(slug);
			if (null == model.Post)
			{
				model.Post = new PostVM();
			}

			var cats = await _blog.GetCategories(false);
			foreach (var cat in cats)
			{
				model.Categories.Add(new CategoryLink()
				{
					Title = cat,
					Link = $"/blog/category/{cat}"
				});
			}

			return model;
		}
	}
}