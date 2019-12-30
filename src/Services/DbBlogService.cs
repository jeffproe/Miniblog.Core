using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Miniblog.Core.Database;
using Miniblog.Core.Database.Models;
using Miniblog.Core.Models;

namespace Miniblog.Core.Services
{
	public class DbBlogService : BaseBlogService, IBlogService
	{
		private readonly BlogContext _context;
		private readonly IMapper _mapper;

		public DbBlogService(BlogContext context, IMapper mapper, IWebHostEnvironment env, IHttpContextAccessor contextAccessor)
			: base(env, contextAccessor)
		{
			_context = context;
			_mapper = mapper;
		}

		public async Task DeletePost(PostVM postvm)
		{
			try
			{
				int id = int.Parse(postvm.Id);

				var post = await _context.Posts
					.Include(x => x.PostCategories)
					.Include(x => x.Comments)
					.SingleOrDefaultAsync(x => x.Id == id);

				if (null != post)
				{
					foreach (var comment in post.Comments)
					{
						_context.Comments.Remove(comment);
					}

					foreach (var pc in post.PostCategories)
					{
						_context.PostCategories.Remove(pc);
					}

					_context.Posts.Remove(post);

					await _context.SaveChangesAsync();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		public Task<IEnumerable<string>> GetCategories()
		{
			var cats = _context.Categories
				.AsNoTracking()
				.Select(x => x.Name.ToLowerInvariant())
				.Distinct()
				.AsEnumerable<string>();

			return Task.FromResult(cats);
		}

		public async Task<PostVM> GetPostById(string id)
		{
			try
			{
				int postId = int.Parse(id);
				var post = await _context.Posts
					.AsNoTracking()
					.Include(x => x.Comments)
					.Include(x => x.PostCategories).ThenInclude(x => x.Category)
					.SingleOrDefaultAsync(x => x.Id == postId);

				if (null != post && post.PubDate <= DateTime.UtcNow && (post.IsPublished || IsAdmin()))
				{
					var postVm = _mapper.Map<PostVM>(post);
					return postVm;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			return null;
		}

		public async Task<PostVM> GetPostBySlug(string slug)
		{
			try
			{
				var post = await _context.Posts
					.AsNoTracking()
					.Include(x => x.Comments)
					.Include(x => x.PostCategories).ThenInclude(x => x.Category)
					.SingleOrDefaultAsync(x => x.Slug.Equals(slug));

				if (null != post && post.PubDate <= DateTime.UtcNow && (post.IsPublished || IsAdmin()))
				{
					var postVm = _mapper.Map<PostVM>(post);
					return postVm;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			return null;
		}

		public async Task<IEnumerable<PostVM>> GetPosts()
		{
			try
			{
				var posts = await _context.Posts
					.AsNoTracking()
					.Where(p => p.PubDate <= DateTime.UtcNow
						&& (p.IsPublished || IsAdmin())
						&& p.Slug != "home" && p.Slug != "about")
					.OrderByDescending(p => p.PubDate)
					.ToListAsync();

				var postVms = _mapper.Map<List<PostVM>>(posts);
				return postVms.AsEnumerable();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return new List<PostVM>().AsEnumerable();
			}
		}

		public async Task<IEnumerable<PostVM>> GetPosts(int count, int skip = 0)
		{
			try
			{
				var posts = await _context.Posts
					.AsNoTracking()
					.Include(x => x.Comments)
					.Include(x => x.PostCategories).ThenInclude(x => x.Category)
					.Where(p => p.PubDate <= DateTime.UtcNow
						&& (p.IsPublished || IsAdmin())
						&& p.Slug != "home" && p.Slug != "about")
					.OrderByDescending(p => p.PubDate)
					.Skip(skip)
					.Take(count)
					.ToListAsync();

				var postVms = _mapper.Map<List<PostVM>>(posts);
				return postVms.AsEnumerable();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return new List<PostVM>().AsEnumerable();
			}
		}

		public Task<IEnumerable<PostVM>> GetPostsByCategory(string category)
		{
			try
			{
				var posts = _context.Posts
					.AsNoTracking()
					.Include(x => x.Comments)
					.Include(x => x.PostCategories).ThenInclude(x => x.Category)
					.Where(x => x.PubDate <= DateTime.UtcNow
						&& (x.IsPublished || IsAdmin())
					   && x.PostCategories.Any(y => y.Category.Name.Equals(category)));

				var postVms = _mapper.Map<List<PostVM>>(posts);
				return Task.FromResult(postVms.AsEnumerable());
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return Task.FromResult(new List<PostVM>().AsEnumerable());
			}
		}

		public async Task<string> SaveFile(byte[] bytes, string fileName, string suffix = null)
		{
			return await SaveFileAsync(bytes, fileName, suffix);
		}

		public async Task SavePost(PostVM postVm)
		{
			try
			{
				int postId = int.Parse(postVm.Id);

				var post = await _context.Posts
					.AsNoTracking()
					.Include(x => x.Comments)
					.Include(x => x.PostCategories).ThenInclude(x => x.Category)
					.SingleOrDefaultAsync(x => x.Id == postId);

				post = _mapper.Map<Post>(postVm);

				if (0 == post.Id)
				{
					_context.Posts.Add(post);
				}
				else
				{
					_context.Entry(post).State = EntityState.Modified;
				}

				await _context.SaveChangesAsync();

				foreach (var comment in post.Comments)
				{
					var c = await _context.Comments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == comment.Id);
					if (null == c)
					{
						_context.Comments.Add(comment);
						await _context.SaveChangesAsync();
					}
				}

				var currentCommentIds = post.Comments.Select(x => x.Id);
				var commentsToRemove = await _context.Comments
					.Where(x => x.PostId == post.Id
						&& !currentCommentIds.Contains(x.Id))
					.ToListAsync();
				if (0 < commentsToRemove.Count)
				{
					_context.Comments.RemoveRange(commentsToRemove);
					await _context.SaveChangesAsync();
				}

				// TODO: is there a better way?
				foreach (var catName in postVm.Categories)
				{
					var cat = await _context.Categories.AsNoTracking().SingleOrDefaultAsync(x => x.Name == catName);
					if (null == cat)
					{
						cat = new Category() { Name = catName };
						_context.Categories.Add(cat);
						await _context.SaveChangesAsync();
					}
					var postCat = await _context.PostCategories.AsNoTracking().SingleOrDefaultAsync(x => x.PostId == post.Id && x.CategoryId == cat.Id);
					if (null == postCat)
					{
						postCat = new PostCategory()
						{
							PostId = post.Id,
							CategoryId = cat.Id,
						};
						_context.PostCategories.Add(postCat);
						await _context.SaveChangesAsync();
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}