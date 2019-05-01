using Microsoft.EntityFrameworkCore.Migrations;

namespace MinecraftWrapper.Data.Migrations
{
    public partial class LookupSeed1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable (
                name: "StoreItemType",
                columns: table => new
                {
                    StoreItemTypeId = table.Column<int> ( nullable: false ),
                    Description = table.Column<string> ( nullable: false, maxLength: 50 )
                },
                constraints: table =>
                {
                    table.PrimaryKey ( "PK_StoreItemType", x => x.StoreItemTypeId );
                }
                );

            migrationBuilder.AddForeignKey ( name: "FK_StoreItem_StoreItemType_StoreItemTypeId", 
                                             table:"StoreItem", 
                                             column:"StoreItemTypeId", 
                                             principalTable:"StoreItemType", 
                                             principalColumn: "StoreItemTypeId" );

            migrationBuilder.InsertData ( table: "CurrencyType", columns: new string [] { "CurrencyTypeId", "Description" }, values: new object [] { 1, "Normal" } );
            migrationBuilder.InsertData ( table: "CurrencyType", columns: new string [] { "CurrencyTypeId", "Description" }, values: new object [] { 2, "Gift" } );

            migrationBuilder.InsertData ( table: "CurrencyTransactionReason", columns: new string [] { "CurrencyTransactionReasonId", "Description" }, values: new object [] { 1, "Discord Message" } );
            migrationBuilder.InsertData ( table: "CurrencyTransactionReason", columns: new string [] { "CurrencyTransactionReasonId", "Description" }, values: new object [] { 2, "Purchase" } );
            migrationBuilder.InsertData ( table: "CurrencyTransactionReason", columns: new string [] { "CurrencyTransactionReasonId", "Description" }, values: new object [] { 3, "Daily Login" } );
            migrationBuilder.InsertData ( table: "CurrencyTransactionReason", columns: new string [] { "CurrencyTransactionReasonId", "Description" }, values: new object [] { 4, "Gift" } );

            migrationBuilder.InsertData ( table: "StoreItemType", columns: new string [] { "StoreItemTypeId", "Description" }, values: new object [] { 1, "Command" } );
            migrationBuilder.InsertData ( table: "StoreItemType", columns: new string [] { "StoreItemTypeId", "Description" }, values: new object [] { 2, "Membership" } );
            migrationBuilder.InsertData ( table: "StoreItemType", columns: new string [] { "StoreItemTypeId", "Description" }, values: new object [] { 3, "Rank" } );
            migrationBuilder.InsertData ( table: "StoreItemType", columns: new string [] { "StoreItemTypeId", "Description" }, values: new object [] { 4, "Scheduled Command" } );

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable ( "StoreItemType" );

            migrationBuilder.DropForeignKey ( name: "FK_StoreItem_StoreItemType_StoreItemTypeId",
                                              table: "StoreItem" );

            migrationBuilder.DeleteData ( table: "CurrencyType", keyColumn: "CurrencyTypeId", keyValues: new object [] { 1, 2 } );
            migrationBuilder.DeleteData ( table: "CurrencyTransactionReason", keyColumn: "CurrencyTransactionReasonId", keyValues: new object [] { 1, 2, 3, 4 } );
        }
    }
}
