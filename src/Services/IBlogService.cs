using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Miniblog.Core.Models;

namespace Miniblog.Core.Services
{
	public interface IBlogService
	{
		Task<IEnumerable<PostVM>> GetPosts(int count, int skip = 0);

		Task<IEnumerable<PostVM>> GetPostsByCategory(string category);

		Task<PostVM> GetPostBySlug(string slug);

		Task<PostVM> GetPostById(string id);

		Task<IEnumerable<string>> GetCategories();

		Task SavePost(PostVM post);

		Task DeletePost(PostVM post);

		Task<string> SaveFile(byte[] bytes, string fileName, string suffix = null);
	}

	public abstract class InMemoryBlogServiceBase : IBlogService
	{
		public InMemoryBlogServiceBase(IHttpContextAccessor contextAccessor)
		{
			ContextAccessor = contextAccessor;
		}

		protected List<PostVM> Cache { get; set; }
		protected IHttpContextAccessor ContextAccessor { get; }

		public virtual Task<IEnumerable<PostVM>> GetPosts(int count, int skip = 0)
		{
			bool isAdmin = IsAdmin();

			var posts = Cache
				.Where(p => p.PubDate <= DateTime.UtcNow && (p.IsPublished || isAdmin))
				.Skip(skip)
				.Take(count);

			return Task.FromResult(posts);
		}

		public virtual Task<IEnumerable<PostVM>> GetPostsByCategory(string category)
		{
			bool isAdmin = IsAdmin();

			var posts = from p in Cache
						where p.PubDate <= DateTime.UtcNow && (p.IsPublished || isAdmin)
						where p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase)
						select p;

			return Task.FromResult(posts);

		}

		public virtual Task<PostVM> GetPostBySlug(string slug)
		{
			var post = Cache.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
			bool isAdmin = IsAdmin();

			if (post != null && post.PubDate <= DateTime.UtcNow && (post.IsPublished || isAdmin))
			{
				return Task.FromResult(post);
			}

			return Task.FromResult<PostVM>(null);
		}

		public virtual Task<PostVM> GetPostById(string id)
		{
			var post = Cache.FirstOrDefault(p => p.Id == id);
			bool isAdmin = IsAdmin();

			if (post != null && post.PubDate <= DateTime.UtcNow && (post.IsPublished || isAdmin))
			{
				return Task.FromResult(post);
			}

			return Task.FromResult<PostVM>(null);
		}

		public virtual Task<IEnumerable<string>> GetCategories()
		{
			bool isAdmin = IsAdmin();

			var categories = Cache
				.Where(p => p.IsPublished || isAdmin)
				.SelectMany(post => post.Categories)
				.Select(cat => cat.ToLowerInvariant())
				.Distinct();

			return Task.FromResult(categories);
		}

		public abstract Task SavePost(PostVM post);

		public abstract Task DeletePost(PostVM post);

		public abstract Task<string> SaveFile(byte[] bytes, string fileName, string suffix = null);

		protected void SortCache()
		{
			Cache.Sort((p1, p2) => p2.PubDate.CompareTo(p1.PubDate));
		}

		protected bool IsAdmin()
		{
			return ContextAccessor.HttpContext?.User?.Identity.IsAuthenticated == true;
		}
	}
}
