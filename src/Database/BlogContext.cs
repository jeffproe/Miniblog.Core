using Microsoft.EntityFrameworkCore;
using Miniblog.Core.Database.Models;

namespace Miniblog.Core.Database
{
	public class BlogContext : DbContext
	{
		public DbSet<Post> Posts { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<PostCategory> PostCategories { get; set; }

		public BlogContext(DbContextOptions<BlogContext> options)
			: base(options)
		{ }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<PostCategory>().HasKey(pc => new { pc.PostId, pc.CategoryId });

			modelBuilder.Entity<PostCategory>()
				.HasOne<Post>(pc => pc.Post)
				.WithMany(p => p.PostCategories)
				.HasForeignKey(pc => pc.PostId);

			modelBuilder.Entity<PostCategory>()
				.HasOne<Category>(pc => pc.Category)
				.WithMany(c => c.PostCategories)
				.HasForeignKey(pc => pc.CategoryId);
		}
	}
}