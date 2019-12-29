using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Miniblog.Core.Database.Migrations
{
	public partial class Initial : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Categories",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Categories", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Posts",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Title = table.Column<string>(nullable: true),
					Slug = table.Column<string>(nullable: true),
					Excerpt = table.Column<string>(nullable: true),
					Content = table.Column<string>(nullable: true),
					PubDate = table.Column<DateTime>(nullable: false),
					LastModified = table.Column<DateTime>(nullable: false),
					IsPublished = table.Column<bool>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Posts", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Comments",
				columns: table => new
				{
					Id = table.Column<string>(nullable: false),
					Author = table.Column<string>(nullable: true),
					Email = table.Column<string>(nullable: true),
					Content = table.Column<string>(nullable: true),
					PubDate = table.Column<DateTime>(nullable: false),
					IsAdmin = table.Column<bool>(nullable: false),
					PostId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Comments", x => x.Id);
					table.ForeignKey(
						name: "FK_Comments_Posts_PostId",
						column: x => x.PostId,
						principalTable: "Posts",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "PostCategories",
				columns: table => new
				{
					PostId = table.Column<int>(nullable: false),
					CategoryId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PostCategories", x => new { x.PostId, x.CategoryId });
					table.ForeignKey(
						name: "FK_PostCategories_Categories_CategoryId",
						column: x => x.CategoryId,
						principalTable: "Categories",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_PostCategories_Posts_PostId",
						column: x => x.PostId,
						principalTable: "Posts",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Comments_PostId",
				table: "Comments",
				column: "PostId");

			migrationBuilder.CreateIndex(
				name: "IX_PostCategories_CategoryId",
				table: "PostCategories",
				column: "CategoryId");

			migrationBuilder.Sql("INSERT INTO [dbo].Posts" +
				"	(Title, Slug, PubDate, LastModified, Excerpt, Content, IsPublished)" +
				"VALUES" +
				"	( 'Welcome', 'home', GETDATE(), GETDATE(), 'Hello from our editable Home!', '<p>This is stuff you should know about us!</p>" +
				"	<p>And you should read our <a href=\"/blog\">Blog</a>!</p>" +
				"	<p>Spicy jalapeno bacon ipsum dolor amet doner hamburger tempor sed aliqua. Cow adipisicing andouille sausage, ullamco ea reprehenderit ut doner aliqua. Commodo tenderloin meatloaf in prosciutto, drumstick aliqua eiusmod. Minim aliqua ex non duis ground round anim nisi burgdoggen. Boudin spare ribs fatback ground round cupim ham doner tri-tip labore pork belly pancetta ut rump. Filet mignon alcatra biltong do mollit. Dolore magna sed cillum sunt dolor.</p>" +
				"	<p>Dolore adipisicing salami dolore, rump sint bresaola pork short loin chislic venison qui ea jowl. Capicola ex est dolore reprehenderit, chuck aute buffalo consectetur adipisicing kielbasa spare ribs flank elit occaecat. Cupim id culpa tempor doner. Flank adipisicing chislic ham hock andouille ipsum buffalo, swine occaecat nisi. Occaecat dolore fatback nulla shank shoulder. Est laboris shankle esse.</p>'," +
				"	1" +
				")");

			migrationBuilder.Sql(@"INSERT INTO [dbo].Posts
    (Title, Slug, PubDate, LastModified, Excerpt, Content, IsPublished)
VALUES
    ('Our Story', 'about', GETDATE(), GETDATE(), 'Now, this is a story all about how my life got flipped-turned upside down and I''d like to take a minute, just sit right there, I''ll tell you how I became the prince of a town called Bel Air',
      'Now, this is a story all about how<br />My life got flipped-turned upside down<br />And I''d like to take a minute<br />Just sit right there<br />I''ll tell you how I became the prince of a town called Bel Air',
    1
  )");

			migrationBuilder.Sql(@"INSERT INTO [dbo].Posts
    (Title, Slug, PubDate, LastModified, Excerpt, Content, IsPublished)
VALUES
    ('Welcome to your new blog', 'welcome', GETDATE(), GETDATE(), 'Congratulations on your new blog',
      '<p>Congratulations on installing your new blog!</p>" +
"<p><a href=\"https://github.com/madskristensen/Miniblog.Core\">Miniblog.Core</a> is optimized to give your visitors a fantastic reading experience on all types of devices. It is built to be fast and responsive so your visitors never have to wait for the content to load.</p>" +
@"<h2>Fonts and layout</h2>
<p>Instead of using custom fonts that visitors need to download, Miniblog.Core uses fonts native to each device and operating system. This ensures that the blog loads very fast and the fonts feel natural and beautiful on all devices.</p>
<blockquote>
<p>Common text layouts are being styled to look great. This is an example of what a blockquote looks like.</p>
</blockquote>
<p>Here is a list of just some of the features:</p>
<ul>
<li>Social media integration</li>
<li>User comments</li>" +
"<li>Passing <a href=\"http://webdevchecklist.com/\">Web Developer Checklist</a></li>" +
@"<li>Search engine optimized</li>
<li>Supported in all major browsers</li>
<li>Phone and tablet support</li>
<li>Fast and responsive</li>
<li>RSS/ATOM feeds</li>
<li>Windows/Open Live Writer/Markdown Monster support</li>
</ul>
<h2>Special formatting</h2>
<p>Pre-formatted text such as what is commonly used to display code syntax is styled beautifully.</p>
<pre><code>function code() {" +
"   var msg = \"This is what a code snippet looks like\";" +
@"}</code></pre>
<p>And tables are formatted nicely with headings and alternative background color for rows.</p>
<table>
<tbody>
<tr>
<th>And tables</th>
<th>look</th>
<th>rally</th>
<th>nice</th>
<th>too</th>
</tr>
<tr>
<td>Numbers</td>
<td>2</td>
<td>3</td>
<td>4</td>
<td>5</td>
</tr>
<tr>
<td>Alphabet</td>
<td>b</td>
<td>c</td>
<td>d</td>
<td>e</td>
</tr>
<tr>
<td>Symbols</td>
<td>?</td>
<td>$</td>
<td>*</td>
<td>%</td>
</tr>
</tbody>
</table>
<p>Enjoy!</p>',
    1
  )");
			migrationBuilder.Sql(@"INSERT INTO [dbo].Comments
    (Id, Author, Email, Content, PubDate, IsAdmin, PostId)
VALUES
    (NEWID(), 'John Doe', 'john@gmail.com', 'This is a comment made by a visitor', GETDATE(), 0, 3)");

			migrationBuilder.Sql(@"INSERT INTO [dbo].Comments
    (Id, Author, Email, Content, PubDate, IsAdmin, PostId)
VALUES
    (NEWID(), 'Site Admin', 'site.admin@gmail.com', 'This is a comment made by the owner of the blog while logged in.

It looks slightly different to make it clear that this is a response from the owner.', GETDATE(), 1, 3)");

			migrationBuilder.Sql(@"INSERT INTO dbo.Categories (Name) VALUES ('Welcome')");

			migrationBuilder.Sql(@"INSERT INTO [dbo].[PostCategories] ([PostId], [CategoryId]) VALUES (3, 1)");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Comments");

			migrationBuilder.DropTable(
				name: "PostCategories");

			migrationBuilder.DropTable(
				name: "Categories");

			migrationBuilder.DropTable(
				name: "Posts");
		}
	}
}
