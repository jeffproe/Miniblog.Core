using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Miniblog.Core.Services
{
	public class BaseBlogService
	{
		private readonly IHttpContextAccessor _contextAccessor;
		private const string POSTS = "Posts";
		private const string FILES = "files";
		protected readonly string _folder;

		public BaseBlogService(IWebHostEnvironment env, IHttpContextAccessor contextAccessor)
		{
			_contextAccessor = contextAccessor;
			_folder = Path.Combine(env.WebRootPath, POSTS);
		}

		protected static string CleanFromInvalidChars(string input)
		{
			// ToDo: what we are doing here if we switch the blog from windows
			// to unix system or vice versa? we should remove all invalid chars for both systems

			var regexSearch = Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()));
			var r = new Regex($"[{regexSearch}]");
			return r.Replace(input, "");
		}

		protected bool IsAdmin()
		{
			return _contextAccessor.HttpContext?.User?.Identity.IsAuthenticated == true;
		}

		protected async Task<string> SaveFileAsync(byte[] bytes, string fileName, string suffix = null)
		{
			suffix = CleanFromInvalidChars(suffix ?? DateTime.UtcNow.Ticks.ToString());

			string ext = Path.GetExtension(fileName);
			string name = CleanFromInvalidChars(Path.GetFileNameWithoutExtension(fileName));

			string fileNameWithSuffix = $"{name}_{suffix}{ext}";

			string absolute = Path.Combine(_folder, FILES, fileNameWithSuffix);
			string dir = Path.GetDirectoryName(absolute);

			Directory.CreateDirectory(dir);
			using (var writer = new FileStream(absolute, FileMode.CreateNew))
			{
				await writer.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
			}

			return $"/{POSTS}/{FILES}/{fileNameWithSuffix}";
		}
	}
}