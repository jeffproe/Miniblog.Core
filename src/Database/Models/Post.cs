using System;
using System.Collections.Generic;

namespace Miniblog.Core.Database.Models
{
	public class Post
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Slug { get; set; }
		public string Excerpt { get; set; }
		public string Content { get; set; }
		public DateTime PubDate { get; set; }
		public DateTime LastModified { get; set; }
		public bool IsPublished { get; set; }
		public virtual IList<PostCategory> PostCategories { get; set; }
		public virtual IList<Comment> Comments { get; set; }
	}
}