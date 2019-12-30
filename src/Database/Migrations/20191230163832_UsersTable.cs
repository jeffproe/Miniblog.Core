using Microsoft.EntityFrameworkCore.Migrations;

namespace Miniblog.Core.Database.Migrations
{
	public partial class UsersTable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Users",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					UserName = table.Column<string>(nullable: true),
					Password = table.Column<string>(nullable: true),
					DisplayName = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Users", x => x.Id);
				});

			migrationBuilder.Sql(@"INSERT INTO Users
	(UserName, Password, DisplayName)
VALUES
	('demo', 'EB53D045EB132825A39F59AEA3FC453F216CB088775D6E7CE4A9740611B573CD', 'Demo Admin')");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Users");
		}
	}
}
