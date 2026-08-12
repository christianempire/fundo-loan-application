using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fundo.Loans.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SsnHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SsnLast4 = table.Column<string>(type: "TEXT", maxLength: 4, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    city = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    postal_code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    state = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    street = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "loan_applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loan_applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AttemptCount = table.Column<int>(type: "INTEGER", nullable: false),
                    NextAttemptAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastError = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_customers_SsnHash",
                table: "customers",
                column: "SsnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loan_applications_CustomerId",
                table: "loan_applications",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_ProcessedAt_OccurredAt",
                table: "outbox_messages",
                columns: new[] { "ProcessedAt", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "loan_applications");

            migrationBuilder.DropTable(
                name: "outbox_messages");
        }
    }
}
