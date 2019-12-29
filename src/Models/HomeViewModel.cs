using System.Collections.Generic;

namespace Miniblog.Core.Models
{
	public class HomeViewModel
	{
		public HomeViewModel()
		{
			LatestPosts = new List<LatestPost>();
		}

		public List<LatestPost> LatestPosts { get; set; }
		public PostVM Post { get; set; }
	}
}