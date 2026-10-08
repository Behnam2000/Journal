-- ============================================================================
-- Crypto Trading Journal — Option A: materialized tables + triggers
-- Follows your ER diagram's table/column names (day_ov, trade_info, day_sum,
-- trade_log). trade_log and day_sum are real tables here, kept in sync by
-- trigger functions whenever trade_info or day_ov changes.
--
-- Fixes applied vs. the diagram (flagged inline with "FIX:"):
--   - overall_trend:  boolean -> enum (Bull/Bear/Range) — a bool can't hold 3 states
--   - starting_balance / ending_balance: int -> numeric(14,2) — ints can't hold cents
--   - best_trade_pair / worst_trade_pair: decimal -> bigint FK to trade_info(id),
--     renamed best_trade_id / worst_trade_id — a pair name/reference isn't a decimal
--   - day: now auto-filled from the date (generated column)
--   - exit: renamed exit_actual, for symmetry with entry_actual
--   - trade_log gains risk_amount / risk_pct — these existed in the spreadsheet
--     version but were missing from the diagram
--   - mp_state enum values and timeframe enum values weren't specified on the
--     diagram — I picked reasonable defaults; adjust the CREATE TYPE lists below
--     to match what you actually want to log
-- ============================================================================

-- ---------------------------------------------------------------------------
-- Enum types
-- ---------------------------------------------------------------------------
CREATE TYPE trend_enum       AS ENUM ('Bull','Bear','Range');
CREATE TYPE volatility_enum  AS ENUM ('Low','Medium','High');
CREATE TYPE mp_state_enum    AS ENUM ('Great','Good','Tired','Stressed','Distracted','Other');  -- adjust to taste
CREATE TYPE direction_enum   AS ENUM ('Long','Short');
CREATE TYPE setup_enum       AS ENUM ('Breakout','Trend-follow','Scalp','Mean-reversion','News','Other');
CREATE TYPE timeframe_enum   AS ENUM ('1m','5m','15m','1H','4H','1D','Other');                  -- adjust to taste
CREATE TYPE exit_reason_enum AS ENUM ('Stop hit','Target hit','Manual close','Trailing stop','Time-based');
CREATE TYPE psy_state_enum   AS ENUM ('Calm','FOMO','Revenge','Confident','Anxious','Other');

-- ---------------------------------------------------------------------------
-- day_ov — one row per trading day (inputs only; ending_balance is
-- maintained by the trigger chain below, not entered by hand)
-- ---------------------------------------------------------------------------
CREATE TABLE day_ov (
    date              DATE PRIMARY KEY,
    day               VARCHAR(13) GENERATED ALWAYS AS (
                          (ARRAY['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'])
                          [EXTRACT(DOW FROM date)::int + 1]
                      ) STORED,                      -- auto-filled from date
    starting_balance  NUMERIC(14,2) NOT NULL,          -- FIX: was int
    ending_balance    NUMERIC(14,2),                    -- FIX: was int; now trigger-maintained
    overall_trend     trend_enum,                        -- FIX: was boolean
    volatility        volatility_enum,
    mp_state          mp_state_enum,
    news              VARCHAR(150),
    notes             TEXT
);

-- ---------------------------------------------------------------------------
-- trade_info — one row per trade (inputs only)
-- ---------------------------------------------------------------------------
CREATE TABLE trade_info (
    id             BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    trade_date     DATE NOT NULL REFERENCES day_ov(date),
    trade_number   INT,
    time           TIME,
    pair           VARCHAR(15) NOT NULL,
    exchange       VARCHAR(15),
    direction      direction_enum NOT NULL,
    setup          setup_enum,
    timeframe      timeframe_enum,
    entry_thesis   VARCHAR(150),
    context        VARCHAR(150),
    entry_planned  NUMERIC(14,4),
    sl             NUMERIC(14,4),
    tp             NUMERIC(14,4),
    size           NUMERIC(14,8),
    leverage       NUMERIC(6,2),
    entry_actual   NUMERIC(14,4),
    entry_time     TIME,
    exit_actual    NUMERIC(14,4),                       -- FIX: was "exit"
    exit_time      TIME,
    exit_reason    exit_reason_enum,
    fees           NUMERIC(10,2) DEFAULT 0,
    psy_state      psy_state_enum,
    followed_rules BOOLEAN,
    mistakes       VARCHAR(100),
    went_well      VARCHAR(100),
    takeaways      VARCHAR(100),
    chart_rf       BYTEA,
    tags           VARCHAR(100)
);

CREATE INDEX idx_trade_info_date ON trade_info(trade_date);

-- ---------------------------------------------------------------------------
-- trade_log — one row per trade, computed values only (1:1 with trade_info,
-- shares its primary key)
-- ---------------------------------------------------------------------------
CREATE TABLE trade_log (
    id          BIGINT PRIMARY KEY REFERENCES trade_info(id) ON DELETE CASCADE,
    risk_amount NUMERIC(14,2),                           -- NEW: was missing from diagram
    risk_pct    NUMERIC(10,6),                            -- NEW: was missing from diagram
    rr_planned  NUMERIC(10,4),
    pl_amount   NUMERIC(14,2),
    pl_percent  NUMERIC(10,6),
    realized_r  NUMERIC(10,4),
    outcome     VARCHAR(10)
);

-- ---------------------------------------------------------------------------
-- day_sum — one row per day, computed values only (1:1 with day_ov)
-- ---------------------------------------------------------------------------
CREATE TABLE day_sum (
    date               DATE PRIMARY KEY REFERENCES day_ov(date),
    total_trades       SMALLINT,
    wins               SMALLINT,
    losses             SMALLINT,
    net_pl_amount      NUMERIC(14,2),
    win_rate           NUMERIC(6,4),
    net_pl_percent     NUMERIC(10,6),
    best_trade_amount  NUMERIC(14,2),
    best_trade_id      BIGINT REFERENCES trade_info(id) ON DELETE SET NULL,   -- FIX: was best_trade_pair decimal
    worst_trade_amount NUMERIC(14,2),
    worst_trade_id     BIGINT REFERENCES trade_info(id) ON DELETE SET NULL,   -- FIX: was worst_trade_pair decimal
    followed_plan_ov   VARCHAR(100),
    lesson             VARCHAR,
    to_improve         VARCHAR
);

-- ============================================================================
-- Trigger functions
-- ============================================================================

-- Recompute one trade_log row from its trade_info row + that day's starting
-- balance. Called whenever a trade is inserted/updated, or when the day's
-- starting balance changes (risk_pct depends on it).
CREATE OR REPLACE FUNCTION recalc_trade(p_id BIGINT) RETURNS VOID AS $$
DECLARE
    t trade_info%ROWTYPE;
    v_starting_balance NUMERIC;
    v_risk_amount NUMERIC;
    v_risk_pct NUMERIC;
    v_rr_planned NUMERIC;
    v_pl_amount NUMERIC;
    v_pl_percent NUMERIC;
    v_realized_r NUMERIC;
    v_outcome TEXT;
BEGIN
    SELECT * INTO t FROM trade_info WHERE id = p_id;
    IF NOT FOUND THEN RETURN; END IF;

    SELECT starting_balance INTO v_starting_balance FROM day_ov WHERE date = t.trade_date;

    v_risk_amount := CASE WHEN t.entry_planned IS NULL OR t.sl IS NULL OR t.size IS NULL THEN NULL
                          ELSE ABS(t.entry_planned - t.sl) * t.size END;

    v_risk_pct := CASE WHEN v_risk_amount IS NULL OR v_starting_balance IS NULL OR v_starting_balance = 0 THEN NULL
                        ELSE v_risk_amount / v_starting_balance END;

    v_rr_planned := CASE WHEN t.entry_planned IS NULL OR t.sl IS NULL OR t.tp IS NULL
                              OR ABS(t.entry_planned - t.sl) = 0 THEN NULL
                          ELSE ABS(t.tp - t.entry_planned) / ABS(t.entry_planned - t.sl) END;

    v_pl_amount := CASE
                      WHEN t.entry_actual IS NULL OR t.exit_actual IS NULL THEN NULL
                      WHEN t.direction = 'Long'  THEN (t.exit_actual - t.entry_actual) * t.size - COALESCE(t.fees,0)
                      WHEN t.direction = 'Short' THEN (t.entry_actual - t.exit_actual) * t.size - COALESCE(t.fees,0)
                   END;

    v_pl_percent := CASE WHEN v_pl_amount IS NULL OR t.entry_actual IS NULL OR t.entry_actual * t.size = 0 THEN NULL
                          ELSE v_pl_amount / (t.entry_actual * t.size) END;

    v_realized_r := CASE WHEN v_pl_amount IS NULL OR v_risk_amount IS NULL OR v_risk_amount = 0 THEN NULL
                          ELSE v_pl_amount / v_risk_amount END;

    v_outcome := CASE WHEN v_pl_amount IS NULL THEN NULL
                      WHEN v_pl_amount > 0 THEN 'Win'
                      WHEN v_pl_amount < 0 THEN 'Loss'
                      ELSE 'Breakeven' END;

    INSERT INTO trade_log (id, risk_amount, risk_pct, rr_planned, pl_amount, pl_percent, realized_r, outcome)
    VALUES (t.id, v_risk_amount, v_risk_pct, v_rr_planned, v_pl_amount, v_pl_percent, v_realized_r, v_outcome)
    ON CONFLICT (id) DO UPDATE SET
        risk_amount = EXCLUDED.risk_amount,
        risk_pct    = EXCLUDED.risk_pct,
        rr_planned  = EXCLUDED.rr_planned,
        pl_amount   = EXCLUDED.pl_amount,
        pl_percent  = EXCLUDED.pl_percent,
        realized_r  = EXCLUDED.realized_r,
        outcome     = EXCLUDED.outcome;
END;
$$ LANGUAGE plpgsql;

-- Recompute day_sum + day_ov.ending_balance for one date, from whatever is
-- currently in trade_log for that date. Leaves lesson/to_improve untouched
-- (those are manual fields, not derived).
CREATE OR REPLACE FUNCTION recalc_day(p_date DATE) RETURNS VOID AS $$
DECLARE
    v_starting_balance NUMERIC;
    v_total INT; v_wins INT; v_losses INT; v_net NUMERIC; v_win_rate NUMERIC; v_net_pct NUMERIC;
    v_best_amt NUMERIC; v_best_id BIGINT; v_worst_amt NUMERIC; v_worst_id BIGINT;
    v_followed TEXT;
BEGIN
    SELECT starting_balance INTO v_starting_balance FROM day_ov WHERE date = p_date;
    IF NOT FOUND THEN RETURN; END IF;

    SELECT
        COUNT(*),
        COUNT(*) FILTER (WHERE tl.outcome = 'Win'),
        COUNT(*) FILTER (WHERE tl.outcome = 'Loss'),
        COALESCE(SUM(tl.pl_amount), 0)
    INTO v_total, v_wins, v_losses, v_net
    FROM trade_info ti JOIN trade_log tl ON tl.id = ti.id
    WHERE ti.trade_date = p_date;

    v_win_rate := CASE WHEN v_total = 0 THEN NULL ELSE v_wins::numeric / v_total END;
    v_net_pct  := CASE WHEN v_starting_balance = 0 THEN NULL ELSE v_net / v_starting_balance END;

    SELECT ti.id, tl.pl_amount INTO v_best_id, v_best_amt
    FROM trade_info ti JOIN trade_log tl ON tl.id = ti.id
    WHERE ti.trade_date = p_date AND tl.pl_amount IS NOT NULL
    ORDER BY tl.pl_amount DESC, ti.id LIMIT 1;

    SELECT ti.id, tl.pl_amount INTO v_worst_id, v_worst_amt
    FROM trade_info ti JOIN trade_log tl ON tl.id = ti.id
    WHERE ti.trade_date = p_date AND tl.pl_amount IS NOT NULL
    ORDER BY tl.pl_amount ASC,  ti.id LIMIT 1;

    SELECT CASE WHEN COUNT(*) = 0 THEN NULL
                WHEN BOOL_OR(ti.followed_rules = FALSE) THEN 'No - see Trade Log'
                ELSE 'Yes' END
    INTO v_followed
    FROM trade_info ti WHERE ti.trade_date = p_date;

    INSERT INTO day_sum (date, total_trades, wins, losses, net_pl_amount, win_rate, net_pl_percent,
                          best_trade_amount, best_trade_id, worst_trade_amount, worst_trade_id, followed_plan_ov)
    VALUES (p_date, v_total, v_wins, v_losses, v_net, v_win_rate, v_net_pct,
            v_best_amt, v_best_id, v_worst_amt, v_worst_id, v_followed)
    ON CONFLICT (date) DO UPDATE SET
        total_trades       = EXCLUDED.total_trades,
        wins                = EXCLUDED.wins,
        losses              = EXCLUDED.losses,
        net_pl_amount       = EXCLUDED.net_pl_amount,
        win_rate            = EXCLUDED.win_rate,
        net_pl_percent      = EXCLUDED.net_pl_percent,
        best_trade_amount   = EXCLUDED.best_trade_amount,
        best_trade_id       = EXCLUDED.best_trade_id,
        worst_trade_amount  = EXCLUDED.worst_trade_amount,
        worst_trade_id      = EXCLUDED.worst_trade_id,
        followed_plan_ov    = EXCLUDED.followed_plan_ov;
        -- lesson / to_improve intentionally omitted: manual fields, preserved as-is

    UPDATE day_ov SET ending_balance = v_starting_balance + v_net WHERE date = p_date;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- Triggers — wire the functions above to fire automatically
-- ============================================================================

-- A trade was added or edited: recompute its trade_log row, then its day's summary
CREATE OR REPLACE FUNCTION trg_trade_info_write() RETURNS TRIGGER AS $$
BEGIN
    PERFORM recalc_trade(NEW.id);
    PERFORM recalc_day(NEW.trade_date);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trade_info_write
AFTER INSERT OR UPDATE ON trade_info
FOR EACH ROW EXECUTE FUNCTION trg_trade_info_write();

-- A trade was deleted: its trade_log row cascades automatically (FK above);
-- still need to refresh that day's summary
CREATE OR REPLACE FUNCTION trg_trade_info_delete() RETURNS TRIGGER AS $$
BEGIN
    PERFORM recalc_day(OLD.trade_date);
    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trade_info_delete
AFTER DELETE ON trade_info
FOR EACH ROW EXECUTE FUNCTION trg_trade_info_delete();

-- The day's starting balance changed after trades were already logged:
-- risk_pct on every trade that day depends on it, as does net_pl_percent
CREATE OR REPLACE FUNCTION trg_day_ov_balance_change() RETURNS TRIGGER AS $$
DECLARE r RECORD;
BEGIN
    IF NEW.starting_balance IS DISTINCT FROM OLD.starting_balance THEN
        FOR r IN SELECT id FROM trade_info WHERE trade_date = NEW.date LOOP
            PERFORM recalc_trade(r.id);
        END LOOP;
        PERFORM recalc_day(NEW.date);
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER day_ov_balance_change
AFTER UPDATE OF starting_balance ON day_ov
FOR EACH ROW EXECUTE FUNCTION trg_day_ov_balance_change();
