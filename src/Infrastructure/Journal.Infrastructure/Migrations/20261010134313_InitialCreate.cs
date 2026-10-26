using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Journal.Infrastructure.Persistence;

#nullable disable

namespace Journal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_journals",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    day = table.Column<string>(type: "text", nullable: false),
                    starting_balance = table.Column<decimal>(type: "numeric", nullable: false),
                    ending_balance = table.Column<decimal>(type: "numeric", nullable: true),
                    overall_trend = table.Column<string>(type: "text", nullable: true),
                    volatility = table.Column<string>(type: "text", nullable: true),
                    mp_state = table.Column<string>(type: "text", nullable: true),
                    news = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    total_trades = table.Column<short>(type: "smallint", nullable: false),
                    wins = table.Column<short>(type: "smallint", nullable: false),
                    losses = table.Column<short>(type: "smallint", nullable: false),
                    net_pl_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    win_rate = table.Column<decimal>(type: "numeric", nullable: true),
                    best_trade_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    best_trade_pair = table.Column<string>(type: "text", nullable: true),
                    worst_trade_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    worst_trade_pair = table.Column<string>(type: "text", nullable: true),
                    followed_plan_ov = table.Column<string>(type: "text", nullable: true),
                    lesson = table.Column<string>(type: "text", nullable: true),
                    to_improve = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_journals", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "trades",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trade_number = table.Column<int>(type: "integer", nullable: false),
                    pair = table.Column<string>(type: "text", nullable: false),
                    exchange = table.Column<string>(type: "text", nullable: false),
                    direction = table.Column<string>(type: "text", nullable: false),
                    setup = table.Column<string>(type: "text", nullable: false),
                    time_frame = table.Column<string>(type: "text", nullable: false),
                    entry_thesis = table.Column<string>(type: "text", nullable: false),
                    context = table.Column<string>(type: "text", nullable: false),
                    entry_planned = table.Column<decimal>(type: "numeric", nullable: false),
                    sl = table.Column<decimal>(type: "numeric", nullable: false),
                    tp = table.Column<decimal>(type: "numeric", nullable: false),
                    size = table.Column<decimal>(type: "numeric", nullable: false),
                    leverage = table.Column<decimal>(type: "numeric", nullable: false),
                    entry_actual = table.Column<decimal>(type: "numeric", nullable: false),
                    entry_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    exit = table.Column<decimal>(type: "numeric", nullable: true),
                    exit_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    exit_reason = table.Column<string>(type: "text", nullable: true),
                    fees = table.Column<decimal>(type: "numeric", nullable: true),
                    psy_state = table.Column<string>(type: "text", nullable: false),
                    followed_rules = table.Column<bool>(type: "boolean", nullable: true),
                    mistakes = table.Column<string>(type: "text", nullable: true),
                    went_well = table.Column<string>(type: "text", nullable: true),
                    take_aways = table.Column<string>(type: "text", nullable: true),
                    chart_rf = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    risk_percent = table.Column<decimal>(type: "numeric", nullable: false),
                    risk_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    rr_planned = table.Column<decimal>(type: "numeric", nullable: false),
                    pl_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    pl_percent = table.Column<decimal>(type: "numeric", nullable: true),
                    realized_r = table.Column<decimal>(type: "numeric", nullable: true),
                    outcome = table.Column<string>(type: "text", nullable: true),
                    daily_journal_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trades", x => x.id);
                    table.ForeignKey(
                        name: "fk_trades_daily_journals_daily_journal_id",
                        column: x => x.daily_journal_id,
                        principalTable: "daily_journals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_trades_daily_journal_id",
                table: "trades",
                column: "daily_journal_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trades");

            migrationBuilder.DropTable(
                name: "daily_journals");
        }
    }
}
