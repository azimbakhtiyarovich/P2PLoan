using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PLoan.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditScoringAndFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── BorrowerProfile: IncomeLevel (string) ni olib, structured fieldlarni qo'shish ──

            migrationBuilder.DropColumn(
                name: "IncomeLevel",
                table: "BorrowerProfiles");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyIncome",
                table: "BorrowerProfiles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExistingDebt",
                table: "BorrowerProfiles",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreditScore",
                table: "BorrowerProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "CreditRating",
                table: "BorrowerProfiles",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastScoredAt",
                table: "BorrowerProfiles",
                type: "datetimeoffset",
                nullable: true);

            // ── Wallet: RowVersion (optimistic concurrency) ──────────────────────

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Wallet",
                type: "rowversion",
                rowVersion: true,
                nullable: false);

            // ── KycDocument: FilePath MaxLength(500) ─────────────────────────────

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "KycDocument",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // ── Indexes ──────────────────────────────────────────────────────────

            migrationBuilder.CreateIndex(
                name: "IX_Loans_Status",
                table: "Loans",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId_Read",
                table: "Notification",
                columns: new[] { "UserId", "Read" });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_WalletId_CreatedAt",
                table: "Transaction",
                columns: new[] { "WalletId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Payment_ExternalId",
                table: "Payment",
                column: "ExternalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Loans_Status", table: "Loans");
            migrationBuilder.DropIndex(name: "IX_Notification_UserId_Read", table: "Notification");
            migrationBuilder.DropIndex(name: "IX_Transaction_WalletId_CreatedAt", table: "Transaction");
            migrationBuilder.DropIndex(name: "IX_Payment_ExternalId", table: "Payment");

            migrationBuilder.DropColumn(name: "RowVersion",    table: "Wallet");
            migrationBuilder.DropColumn(name: "MonthlyIncome", table: "BorrowerProfiles");
            migrationBuilder.DropColumn(name: "ExistingDebt",  table: "BorrowerProfiles");
            migrationBuilder.DropColumn(name: "CreditScore",   table: "BorrowerProfiles");
            migrationBuilder.DropColumn(name: "CreditRating",  table: "BorrowerProfiles");
            migrationBuilder.DropColumn(name: "LastScoredAt",  table: "BorrowerProfiles");

            migrationBuilder.AddColumn<string>(
                name: "IncomeLevel",
                table: "BorrowerProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "KycDocument",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
