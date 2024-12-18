using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetricTesting2.Migrations
{
  /// <inheritdoc />
  public partial class userrole : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<string>(
          name: "Discriminator",
          table: "AspNetRoles",
          type: "TEXT",
          maxLength: 13,
          nullable: false,
          defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "Discriminator",
          table: "AspNetRoles");
    }
  }
}
