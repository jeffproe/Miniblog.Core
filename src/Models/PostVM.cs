﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Miniblog.Core.Models
{
	public class PostVM
	{
		[Required]
		public string Id { get; set; }

		[Required]
		public string Title { get; set; } = string.Empty;

		public string Slug { get; set; } = string.Empty;

		[Required]
		public string Excerpt { get; set; } = string.Empty;

		[Required]
		public string Content { get; set; } = string.Empty;

		public DateTime PubDate { get; set; } = DateTime.UtcNow;

		public DateTime LastModified { get; set; } = DateTime.UtcNow;

		public bool IsPublished { get; set; } = true;

		public IList<string> Categories { get; set; } = new List<string>();

		public IList<CommentVM> Comments { get; } = new List<CommentVM>();

		public string GetLink()
		{
			return $"/blog/{Slug}/";
		}

		public string GetEncodedLink()
		{
			var link = $"/blog/{System.Net.WebUtility.UrlEncode(Slug)}/";
			return link.Replace("+", "%20");
		}

		public bool AreCommentsOpen(int commentsCloseAfterDays)
		{
			return PubDate.AddDays(commentsCloseAfterDays) >= DateTime.UtcNow;
		}

		public static string CreateSlug(string title)
		{
			title = title.ToLowerInvariant().Replace(" ", "-");
			title = RemoveDiacritics(title);
			title = RemoveReservedUrlCharacters(title);

			return title.ToLowerInvariant();
		}

		private static string RemoveReservedUrlCharacters(string text)
		{
			var reservedCharacters = new List<string> { "!", "#", "$", "&", "'", "(", ")", "*", ",", "/", ":", ";", "=", "?", "@", "[", "]", "\"", "%", ".", "<", ">", "\\", "^", "_", "'", "{", "}", "|", "~", "`", "+" };

			foreach (var chr in reservedCharacters)
			{
				text = text.Replace(chr, "");
			}

			return text;
		}

		private static string RemoveDiacritics(string text)
		{
			var normalizedString = text.Normalize(NormalizationForm.FormD);
			var stringBuilder = new StringBuilder();

			foreach (var c in normalizedString)
			{
				var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark)
				{
					stringBuilder.Append(c);
				}
			}

			return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
		}

		public string RenderContent()
		{
			var result = Content;

			// Set up lazy loading of images/iframes
			if (!string.IsNullOrEmpty(result))
			{
				// Set up lazy loading of images/iframes
				var replacement = " src=\"data:image/gif;base64,R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==\" data-src=\"";
				var pattern = "(<img.*?)(src=[\\\"|'])(?<src>.*?)([\\\"|'].*?[/]?>)";
				result = Regex.Replace(result, pattern, m => m.Groups[1].Value + replacement + m.Groups[4].Value + m.Groups[3].Value);

				// Youtube content embedded using this syntax: [youtube:xyzAbc123]
				var video = "<div class=\"video\"><iframe width=\"560\" height=\"315\" title=\"YouTube embed\" src=\"about:blank\" data-src=\"https://www.youtube-nocookie.com/embed/{0}?modestbranding=1&amp;hd=1&amp;rel=0&amp;theme=light\" allowfullscreen></iframe></div>";
				result = Regex.Replace(result, @"\[youtube:(.*?)\]", m => string.Format(video, m.Groups[1].Value));
			}
			return result;
		}
	}
}
