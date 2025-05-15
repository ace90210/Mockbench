using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mockbench.Data.Postgres.AuthenticationMigrations
{
    /// <inheritdoc />
    public partial class CreateIdentitySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Environments",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DefaultHealthCheckUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SimulateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Environments", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Microservices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TargetUrl = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    FakeDelay = table.Column<int>(type: "INTEGER", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ProxyMode = table.Column<int>(type: "INTEGER", nullable: false),
                    SimulateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RandomiseMockResult = table.Column<bool>(type: "INTEGER", nullable: false),
                    PassThroughTenant = table.Column<bool>(type: "boolean", nullable: false),
                    HeadersMode = table.Column<int>(type: "INTEGER", nullable: false),
                    InjectForwardingHeadersOnRequest = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Microservices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PreferredTheme = table.Column<string>(type: "TEXT", nullable: false),
                    UIMode = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SimulateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnvironmentVariables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    EnvironmentId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvironmentVariables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnvironmentVariables_Environments_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "Environments",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Incoming = table.Column<bool>(type: "INTEGER", nullable: false),
                    Outgoing = table.Column<bool>(type: "INTEGER", nullable: false),
                    MicroserviceID = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceHeaders_Microservices_MicroserviceID",
                        column: x => x.MicroserviceID,
                        principalTable: "Microservices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Endpoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FromUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ExactUrlMatch = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExpectAuthHeader = table.Column<bool>(type: "INTEGER", nullable: false),
                    MockBehaviour = table.Column<int>(type: "INTEGER", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RestType = table.Column<int>(type: "INTEGER", nullable: false),
                    SimulateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FromBody = table.Column<string>(type: "TEXT", nullable: true),
                    TTLTicks = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: true),
                    EnvironmentId = table.Column<int>(type: "INTEGER", nullable: true),
                    MicroserviceId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Endpoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Endpoints_Environments_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "Environments",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Endpoints_Microservices_MicroserviceId",
                        column: x => x.MicroserviceId,
                        principalTable: "Microservices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Endpoints_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TenantVariables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantVariables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantVariables_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EndpointHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    EndpointId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndpointHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EndpointHeaders_Endpoints_EndpointId",
                        column: x => x.EndpointId,
                        principalTable: "Endpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MockResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    Code = table.Column<int>(type: "INTEGER", nullable: false),
                    Encoding = table.Column<int>(type: "INTEGER", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Body = table.Column<string>(type: "TEXT", nullable: true),
                    EndpointId = table.Column<int>(type: "INTEGER", nullable: false),
                    Checksum = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    FakeDelay = table.Column<int>(type: "INTEGER", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Latency = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MockResponses_Endpoints_EndpointId",
                        column: x => x.EndpointId,
                        principalTable: "Endpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QueryParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Value = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    EndpointId = table.Column<int>(type: "INTEGER", nullable: false),
                    Ignore = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueryParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueryParameters_Endpoints_EndpointId",
                        column: x => x.EndpointId,
                        principalTable: "Endpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResponseHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    MockResponseID = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponseHeaders_MockResponses_MockResponseID",
                        column: x => x.MockResponseID,
                        principalTable: "MockResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EndpointHeaders_EndpointId",
                table: "EndpointHeaders",
                column: "EndpointId");

            migrationBuilder.CreateIndex(
                name: "IX_Endpoints_EnvironmentId",
                table: "Endpoints",
                column: "EnvironmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Endpoints_MicroserviceId",
                table: "Endpoints",
                column: "MicroserviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Endpoints_TenantId",
                table: "Endpoints",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EnvironmentVariables_EnvironmentId",
                table: "EnvironmentVariables",
                column: "EnvironmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MockResponses_EndpointId",
                table: "MockResponses",
                column: "EndpointId");

            migrationBuilder.CreateIndex(
                name: "IX_QueryParameters_EndpointId",
                table: "QueryParameters",
                column: "EndpointId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseHeaders_MockResponseID",
                table: "ResponseHeaders",
                column: "MockResponseID");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceHeaders_MicroserviceID",
                table: "ServiceHeaders",
                column: "MicroserviceID");

            migrationBuilder.CreateIndex(
                name: "IX_TenantVariables_TenantId",
                table: "TenantVariables",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EndpointHeaders");

            migrationBuilder.DropTable(
                name: "EnvironmentVariables");

            migrationBuilder.DropTable(
                name: "QueryParameters");

            migrationBuilder.DropTable(
                name: "ResponseHeaders");

            migrationBuilder.DropTable(
                name: "ServiceHeaders");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "TenantVariables");

            migrationBuilder.DropTable(
                name: "MockResponses");

            migrationBuilder.DropTable(
                name: "Endpoints");

            migrationBuilder.DropTable(
                name: "Environments");

            migrationBuilder.DropTable(
                name: "Microservices");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
