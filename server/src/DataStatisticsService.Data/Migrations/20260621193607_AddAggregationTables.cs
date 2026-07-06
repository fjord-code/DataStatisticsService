using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataStatisticsService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAggregationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reading_snapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NumericValue = table.Column<double>(type: "double precision", nullable: true),
                    BoolValue = table.Column<bool>(type: "boolean", nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    LastEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reading_time_buckets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BucketStartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Granularity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SampleCount = table.Column<int>(type: "integer", nullable: false),
                    SumNumeric = table.Column<double>(type: "double precision", nullable: true),
                    AvgNumeric = table.Column<double>(type: "double precision", nullable: true),
                    TrueCount = table.Column<int>(type: "integer", nullable: true),
                    FalseCount = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_time_buckets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reading_snapshots_Type_Name",
                table: "reading_snapshots",
                columns: new[] { "Type", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reading_time_buckets_Type_Name_BucketStartUtc_Granularity",
                table: "reading_time_buckets",
                columns: new[] { "Type", "Name", "BucketStartUtc", "Granularity" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reading_snapshots");

            migrationBuilder.DropTable(
                name: "reading_time_buckets");
        }
    }
}
