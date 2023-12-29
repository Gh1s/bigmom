using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace Csb.BigMom.Infrastructure.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data_trace",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    payload = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_trace", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "integration_trace",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    payload = table.Column<string>(type: "json", nullable: false),
                    error = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    guid = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    index = table.Column<long>(type: "bigint", nullable: true),
                    total = table.Column<long>(type: "bigint", nullable: true),
                    table = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    op_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_integration_trace", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mcc",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    range_start = table.Column<TimeSpan>(type: "interval", nullable: false),
                    range_end = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mcc", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "data_trace_response",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    trace_id = table.Column<string>(type: "character varying(36)", nullable: true),
                    status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_trace_response", x => x.id);
                    table.ForeignKey(
                        name: "FK_data_trace_response_data_trace_trace_id",
                        column: x => x.trace_id,
                        principalTable: "data_trace",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "commerce",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    identifiant = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    mcc_id = table.Column<int>(type: "integer", nullable: false),
                    nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    ef = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    date_affiliation = table.Column<DateTime>(type: "date", nullable: false),
                    date_resiliation = table.Column<DateTime>(type: "date", nullable: true),
                    hash = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commerce", x => x.id);
                    table.ForeignKey(
                        name: "FK_commerce_mcc_mcc_id",
                        column: x => x.mcc_id,
                        principalTable: "mcc",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contrat",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    no_contrat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    code = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    date_debut = table.Column<DateTime>(type: "date", nullable: false),
                    date_fin = table.Column<DateTime>(type: "date", nullable: true),
                    commerce_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contrat", x => x.id);
                    table.ForeignKey(
                        name: "FK_contrat_commerce_commerce_id",
                        column: x => x.commerce_id,
                        principalTable: "commerce",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tpe",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    no_serie = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    no_site = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    modele = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    statut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    type_connexion = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    commerce_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tpe", x => x.id);
                    table.ForeignKey(
                        name: "FK_tpe_commerce_commerce_id",
                        column: x => x.commerce_id,
                        principalTable: "commerce",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "application",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    heure_tlc = table.Column<TimeSpan>(type: "interval", nullable: true),
                    idsa = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    tpe_id = table.Column<int>(type: "integer", nullable: false),
                    contrat_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application", x => x.id);
                    table.ForeignKey(
                        name: "FK_application_contrat_contrat_id",
                        column: x => x.contrat_id,
                        principalTable: "contrat",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_application_tpe_tpe_id",
                        column: x => x.tpe_id,
                        principalTable: "tpe",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "spread_trace",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    application_id = table.Column<int>(type: "integer", nullable: false),
                    heure_tlc = table.Column<TimeSpan>(type: "interval", nullable: true),
                    payload = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spread_trace", x => x.id);
                    table.ForeignKey(
                        name: "FK_spread_trace_application_application_id",
                        column: x => x.application_id,
                        principalTable: "application",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tlc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    processing_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    nb_trs_debit = table.Column<int>(type: "integer", nullable: false),
                    total_debit = table.Column<decimal>(type: "numeric", nullable: false),
                    nb_trs_credit = table.Column<int>(type: "integer", nullable: false),
                    total_credit = table.Column<decimal>(type: "numeric", nullable: false),
                    total_reconcilie = table.Column<decimal>(type: "numeric", nullable: false),
                    app_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tlc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tlc_application_app_id",
                        column: x => x.app_id,
                        principalTable: "application",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "spread_trace_response",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    trace_id = table.Column<string>(type: "character varying(36)", nullable: true),
                    job = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spread_trace_response", x => x.id);
                    table.ForeignKey(
                        name: "FK_spread_trace_response_spread_trace_trace_id",
                        column: x => x.trace_id,
                        principalTable: "spread_trace",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_application_contrat_id",
                table: "application",
                column: "contrat_id");

            migrationBuilder.CreateIndex(
                name: "IX_application_idsa",
                table: "application",
                column: "idsa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_application_tpe_id_contrat_id",
                table: "application",
                columns: new[] { "tpe_id", "contrat_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_commerce_identifiant",
                table: "commerce",
                column: "identifiant",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_commerce_mcc_id",
                table: "commerce",
                column: "mcc_id");

            migrationBuilder.CreateIndex(
                name: "IX_contrat_commerce_id",
                table: "contrat",
                column: "commerce_id");

            migrationBuilder.CreateIndex(
                name: "IX_contrat_no_contrat_code_date_debut",
                table: "contrat",
                columns: new[] { "no_contrat", "code", "date_debut" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_data_trace_response_trace_id",
                table: "data_trace_response",
                column: "trace_id");

            migrationBuilder.CreateIndex(
                name: "IX_mcc_code",
                table: "mcc",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_spread_trace_application_id",
                table: "spread_trace",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_spread_trace_response_trace_id",
                table: "spread_trace_response",
                column: "trace_id");

            migrationBuilder.CreateIndex(
                name: "IX_tlc_app_id",
                table: "tlc",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_tpe_commerce_id",
                table: "tpe",
                column: "commerce_id");

            migrationBuilder.CreateIndex(
                name: "IX_tpe_no_serie",
                table: "tpe",
                column: "no_serie",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_trace_response");

            migrationBuilder.DropTable(
                name: "integration_trace");

            migrationBuilder.DropTable(
                name: "spread_trace_response");

            migrationBuilder.DropTable(
                name: "tlc");

            migrationBuilder.DropTable(
                name: "data_trace");

            migrationBuilder.DropTable(
                name: "spread_trace");

            migrationBuilder.DropTable(
                name: "application");

            migrationBuilder.DropTable(
                name: "contrat");

            migrationBuilder.DropTable(
                name: "tpe");

            migrationBuilder.DropTable(
                name: "commerce");

            migrationBuilder.DropTable(
                name: "mcc");
        }
    }
}
