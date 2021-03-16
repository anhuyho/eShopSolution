using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace eShopSolution.Data.Migrations
{
    public partial class ChangeFileLengthType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AppRoles",
                keyColumn: "Id",
                keyValue: new Guid("8d04dce2-969a-435d-bba4-df3f325983dc"),
                column: "ConcurrencyStamp",
                value: "9f2f0cbe-0e46-4e53-80b4-a7146a1c5d99");

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: new Guid("69bd714f-9576-45ba-b5b7-f00649be00de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "939b9e19-8cf6-4330-aa6c-38eecc3b192c", "AQAAAAEAACcQAAAAEOYxA3/CztFvFWuVl4hSTxkRQ6Oll5A14OCScTYfzZOTZbNTdN3DAdpTymhn6XWVfQ==" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "DateCreated",
                value: new DateTime(2021, 3, 16, 23, 48, 10, 705, DateTimeKind.Local).AddTicks(2482));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AppRoles",
                keyColumn: "Id",
                keyValue: new Guid("8d04dce2-969a-435d-bba4-df3f325983dc"),
                column: "ConcurrencyStamp",
                value: "f5df4437-eb3b-4e43-bffc-aa953aa8bfc9");

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: new Guid("69bd714f-9576-45ba-b5b7-f00649be00de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "c32259d3-7bea-4a98-b906-5970c114bb1d", "AQAAAAEAACcQAAAAEJpSMDbIYudgFyQxVS/Sr16XCeBDYS7MzWm15BOXISkEs9Aa/kC6d5xqE5D1pI3shA==" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "DateCreated",
                value: new DateTime(2021, 3, 16, 23, 6, 46, 278, DateTimeKind.Local).AddTicks(7630));
        }
    }
}
