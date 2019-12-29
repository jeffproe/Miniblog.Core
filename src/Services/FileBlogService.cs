using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Miniblog.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Miniblog.Core.Services
{
	public class FileBlogService : BaseBlogService, IBlogService
	{
		private readonly List<PostVM> _cache = new List<PostVM>();

		public FileBlogService(IWebHostEnvironment env, IHttpContextAccessor contextAccessor)
			: base(env, contextAccessor)
		{
			Initialize();
		}

		public virtual Task<IEnumerable<PostVM>> GetPosts(int count, int skip = 0)
		{
			bool isAdmin = IsAdmin();

			var posts = _cache
				.Where(p => p.PubDate <= DateTime.UtcNow
					&& (p.IsPublished || isAdmin)
					&& p.Slug != "home" && p.Slug != "about")
				.Skip(skip)
				.Take(count);

			return Task.FromResult(posts);
		}

		public virtual Task<IEnumerable<PostVM>> GetPostsByCategory(string category)
		{
			bool isAdmin = IsAdmin();

			var posts = from p in _cache
						where p.PubDate <= DateTime.UtcNow && (p.IsPublished || isAdmin)
						where p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase)
						select p;

			return Task.FromResult(posts);
		}

		public virtual Task<PostVM> GetPostBySlug(string slug)
		{
			var post = _cache.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
			bool isAdmin = IsAdmin();

			if (post != null && post.PubDate <= DateTime.UtcNow && (post.IsPublished || isAdmin))
			{
				return Task.FromResult(post);
			}

			return Task.FromResult<PostVM>(null);
		}

		public virtual Task<PostVM> GetPostById(string id)
		{
			var post = _cache.FirstOrDefault(p => p.Id == id);
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

			var categories = _cache
				.Where(p => p.IsPublished || isAdmin)
				.SelectMany(post => post.Categories)
				.Select(cat => cat.ToLowerInvariant())
				.Distinct();

			return Task.FromResult(categories);
		}

		public async Task SavePost(PostVM post)
		{
			string filePath = GetFilePath(post);
			post.LastModified = DateTime.UtcNow;

			XDocument doc = new XDocument(
							new XElement("post",
								new XElement("title", post.Title),
								new XElement("slug", post.Slug),
								new XElement("pubDate", FormatDateTime(post.PubDate)),
								new XElement("lastModified", FormatDateTime(post.LastModified)),
								new XElement("excerpt", post.Excerpt),
								new XElement("content", post.Content),
								new XElement("ispublished", post.IsPublished),
								new XElement("categories", string.Empty),
								new XElement("comments", string.Empty)
							));

			XElement categories = doc.XPathSelectElement("post/categories");
			foreach (string category in post.Categories)
			{
				categories.Add(new XElement("category", category));
			}

			XElement comments = doc.XPathSelectElement("post/comments");
			foreach (CommentVM comment in post.Comments)
			{
				comments.Add(
					new XElement("comment",
						new XElement("author", comment.Author),
						new XElement("email", comment.Email),
						new XElement("date", FormatDateTime(comment.PubDate)),
						new XElement("content", comment.Content),
						new XAttribute("isAdmin", comment.IsAdmin),
						new XAttribute("id", comment.Id)
					));
			}

			using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
			{
				await doc.SaveAsync(fs, SaveOptions.None, CancellationToken.None).ConfigureAwait(false);
			}

			if (!_cache.Contains(post))
			{
				_cache.Add(post);
				SortCache();
			}
		}

		public Task DeletePost(PostVM post)
		{
			string filePath = GetFilePath(post);

			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}

			if (_cache.Contains(post))
			{
				_cache.Remove(post);
			}

			return Task.CompletedTask;
		}

		public async Task<string> SaveFile(byte[] bytes, string fileName, string suffix = null)
		{
			return await SaveFileAsync(bytes, fileName, suffix);
		}

		private string GetFilePath(PostVM post)
		{
			return Path.Combine(_folder, post.Id + ".xml");
		}

		private void Initialize()
		{
			LoadPosts();
			SortCache();
		}

		private void LoadPosts()
		{
			if (!Directory.Exists(_folder))
				Directory.CreateDirectory(_folder);

			// Can this be done in parallel to speed it up?
			foreach (string file in Directory.EnumerateFiles(_folder, "*.xml", SearchOption.TopDirectoryOnly))
			{
				XElement doc = XElement.Load(file);

				PostVM post = new PostVM
				{
					Id = Path.GetFileNameWithoutExtension(file),
					Title = ReadValue(doc, "title"),
					Excerpt = ReadValue(doc, "excerpt"),
					Content = ReadValue(doc, "content"),
					Slug = ReadValue(doc, "slug").ToLowerInvariant(),
					PubDate = DateTime.Parse(ReadValue(doc, "pubDate")),
					LastModified = DateTime.Parse(ReadValue(doc, "lastModified", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))),
					IsPublished = bool.Parse(ReadValue(doc, "ispublished", "true")),
				};

				LoadCategories(post, doc);
				LoadComments(post, doc);
				_cache.Add(post);
			}
		}

		private static void LoadCategories(PostVM post, XElement doc)
		{
			XElement categories = doc.Element("categories");
			if (categories == null)
				return;

			List<string> list = new List<string>();

			foreach (var node in categories.Elements("category"))
			{
				list.Add(node.Value);
			}

			post.Categories = list.ToArray();
		}

		private static void LoadComments(PostVM post, XElement doc)
		{
			var comments = doc.Element("comments");

			if (comments == null)
				return;

			foreach (var node in comments.Elements("comment"))
			{
				CommentVM comment = new CommentVM()
				{
					Id = ReadAttribute(node, "id"),
					Author = ReadValue(node, "author"),
					Email = ReadValue(node, "email"),
					IsAdmin = bool.Parse(ReadAttribute(node, "isAdmin", "false")),
					Content = ReadValue(node, "content"),
					PubDate = DateTime.Parse(ReadValue(node, "date", "2000-01-01")),
				};

				post.Comments.Add(comment);
			}
		}

		private static string ReadValue(XElement doc, XName name, string defaultValue = "")
		{
			if (doc.Element(name) != null)
				return doc.Element(name)?.Value;

			return defaultValue;
		}

		private static string ReadAttribute(XElement element, XName name, string defaultValue = "")
		{
			if (element.Attribute(name) != null)
				return element.Attribute(name)?.Value;

			return defaultValue;
		}

		private static string FormatDateTime(DateTime dateTime)
		{
			const string UTC = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";

			return dateTime.Kind == DateTimeKind.Utc
				? dateTime.ToString(UTC)
				: dateTime.ToUniversalTime().ToString(UTC);
		}

		protected void SortCache()
		{
			_cache.Sort((p1, p2) => p2.PubDate.CompareTo(p1.PubDate));
		}
	}
}