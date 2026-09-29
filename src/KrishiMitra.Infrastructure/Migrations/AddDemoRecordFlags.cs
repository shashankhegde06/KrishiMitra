using KrishiMitra.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KrishiMitra.Infrastructure.Migrations;

[DbContext(typeof(FarmDbContext))]
[Migration("202609280003_AddDemoRecordFlags")]
public sealed class AddDemoRecordFlags : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(@"
ALTER TABLE Activities ADD COLUMN IsDemo INTEGER NOT NULL DEFAULT 0;
ALTER TABLE Reminders ADD COLUMN IsDemo INTEGER NOT NULL DEFAULT 0;
UPDATE Activities SET IsDemo = 1 WHERE Id = 1;
UPDATE Reminders SET IsDemo = 1 WHERE Id = 1;
");

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(@"
ALTER TABLE Activities DROP COLUMN IsDemo;
ALTER TABLE Reminders DROP COLUMN IsDemo;
");
}
