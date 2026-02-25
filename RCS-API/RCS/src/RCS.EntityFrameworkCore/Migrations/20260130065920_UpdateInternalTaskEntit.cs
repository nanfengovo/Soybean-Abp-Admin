using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RCS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInternalTaskEntit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FlowType",
                table: "tb_InternalTask",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlowType",
                table: "tb_InternalTask");
        }
    }
}
