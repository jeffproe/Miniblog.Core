using System.Collections.Generic;

namespace Miniblog.Core.Database.Models
{
	public class Category
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public virtual IList<PostCategory> PostCategories { get; set; }
	}
}