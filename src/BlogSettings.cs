namespace Miniblog.Core
{
	public class BlogSettings
	{
		public string Owner { get; set; } = "The Owner";
		public int PostsPerPage { get; set; } = 5;
		public int CommentsCloseAfterDays { get; set; } = 10;
	}
}
