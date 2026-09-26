using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Healthcare.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "staff_doctor_id",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "staff_organization_id",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_staff_doctor_id",
                table: "users",
                column: "staff_doctor_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_staff_organization_id",
                table: "users",
                column: "staff_organization_id");

            migrationBuilder.AddForeignKey(
                name: "FK_users_doctors_staff_doctor_id",
                table: "users",
                column: "staff_doctor_id",
                principalTable: "doctors",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_users_organizations_staff_organization_id",
                table: "users",
                column: "staff_organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_doctors_staff_doctor_id",
                table: "users");

            migrationBuilder.DropForeignKey(
                name: "FK_users_organizations_staff_organization_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_staff_doctor_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_staff_organization_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "staff_doctor_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "staff_organization_id",
                table: "users");
        }
    }
}
