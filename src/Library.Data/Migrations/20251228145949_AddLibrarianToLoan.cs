// <copyright file="20251228145949_AddLibrarianToLoan.cs" company="Transilvania University of Brasov">
// Copyright (c) Grozea George. All rights reserved.
// </copyright>

#nullable disable

namespace Library.Data.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class AddLibrarianToLoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Readers_ReaderId",
                table: "Loans");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Readers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Readers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "LibrarianId",
                table: "Loans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Loans_LibrarianId",
                table: "Loans",
                column: "LibrarianId");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Readers_LibrarianId",
                table: "Loans",
                column: "LibrarianId",
                principalTable: "Readers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Readers_ReaderId",
                table: "Loans",
                column: "ReaderId",
                principalTable: "Readers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Readers_LibrarianId",
                table: "Loans");

            migrationBuilder.DropForeignKey(
                name: "FK_Loans_Readers_ReaderId",
                table: "Loans");

            migrationBuilder.DropIndex(
                name: "IX_Loans_LibrarianId",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "LibrarianId",
                table: "Loans");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Readers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Readers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_Readers_ReaderId",
                table: "Loans",
                column: "ReaderId",
                principalTable: "Readers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
