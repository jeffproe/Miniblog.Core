using System.Collections.Generic;

namespace Miniblog.Core.Models
{
	public class HomeViewModel
	{
		public HomeViewModel()
		{
			LatestPosts = new List<LatestPost>();
			Categories = new List<CategoryLink>();
		}

		public List<LatestPost> LatestPosts { get; set; }
		public List<CategoryLink> Categories { get; set; }
		public PostVM Post { get; set; }
	}
}