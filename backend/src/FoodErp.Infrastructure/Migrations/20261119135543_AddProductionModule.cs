using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodErp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutputProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutputQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OutputUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recipes_Products_OutputProductId",
                        column: x => x.OutputProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recipes_Units_OutputUnitId",
                        column: x => x.OutputUnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    InputWarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutputWarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlannedBatches = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ActualBatches = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PlannedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_Warehouses_InputWarehouseId",
                        column: x => x.InputWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_Warehouses_OutputWarehouseId",
                        column: x => x.OutputWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecipeLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeLines_Products_IngredientProductId",
                        column: x => x.IngredientProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecipeLines_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeLines_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductionOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlannedQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionOrderLines_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionOrderLines_Products_IngredientProductId",
                        column: x => x.IngredientProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderLines_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderLines_IngredientProductId",
                table: "ProductionOrderLines",
                column: "IngredientProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderLines_ProductionOrderId",
                table: "ProductionOrderLines",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderLines_UnitId",
                table: "ProductionOrderLines",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_InputWarehouseId",
                table: "ProductionOrders",
                column: "InputWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_OutputWarehouseId",
                table: "ProductionOrders",
                column: "OutputWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_RecipeId",
                table: "ProductionOrders",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_TenantId_OrderNumber",
                table: "ProductionOrders",
                columns: new[] { "TenantId", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_TenantId_Status",
                table: "ProductionOrders",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeLines_IngredientProductId",
                table: "RecipeLines",
                column: "IngredientProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeLines_RecipeId_IngredientProductId",
                table: "RecipeLines",
                columns: new[] { "RecipeId", "IngredientProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeLines_UnitId",
                table: "RecipeLines",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_OutputProductId",
                table: "Recipes",
                column: "OutputProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_OutputUnitId",
                table: "Recipes",
                column: "OutputUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_TenantId_OutputProductId",
                table: "Recipes",
                columns: new[] { "TenantId", "OutputProductId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionOrderLines");

            migrationBuilder.DropTable(
                name: "RecipeLines");

            migrationBuilder.DropTable(
                name: "ProductionOrders");

            migrationBuilder.DropTable(
                name: "Recipes");
        }
    }
}
