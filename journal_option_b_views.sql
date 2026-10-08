-- ============================================================================
-- Crypto Trading Journal — Option B: views (no triggers)
-- Same table/column names as your ER diagram (day_ov, trade_info, day_sum,
-- trade_log), but only day_ov and trade_info are real tables. trade_log and
-- day_sum are VIEWS: recalculated on every read, so they can never go stale.
--
-- Differences from Option A you should know about:
--   - You cannot INSERT into trade_log or day_sum (they're views).
--   - day_sum's two manual fields (lesson, to_improve) are things you type,
--     and a view can't store typed text — so they live on day_ov and the
--     day_sum view just exposes them.
--   - ending_balance isn't stored on day_ov; it's computed in the day_sum view.
--
-- Same fixes as Option A (overall_trend enum, numeric balances, best/worst as
-- trade ids, exit -> exit_actual, risk_amount/risk_pct added). Enum values for
-- mp_state and timeframe were unspecified on your diagram — adjust to taste.
-- ============================================================================

CREATE TYPE trend_enum       AS ENUM ('Bull','Bear','Range');
CREATE TYPE volatility_enum  AS ENUM ('Low','Medium','High');
CREATE TYPE mp_state_enum    AS ENUM ('Great','Good','Tired','Stressed','Distracted','Other');  -- adjust to taste
CREATE TYPE direction_enum   AS ENUM ('Long','Short');
CREATE TYPE setup_enum       AS ENUM ('Breakout','Trend-follow','Scalp','Mean-reversion','News','Other');
CREATE TYPE timeframe_enum   AS ENUM ('1m','5m','15m','1H','4H','1D','Other');                  -- adjust to taste
CREATE TYPE exit_reason_enum AS ENUM ('Stop hit','Target hit','Manual close','Trailing stop','Time-based');
CREATE TYPE psy_state_enum   AS ENUM ('Calm','FOMO','Revenge','Confident','Anxious','Other');

-- ---------------------------------------------------------------------------
-- day_ov — one row per day (inputs, plus the two manual end-of-day notes)
-- ---------------------------------------------------------------------------
CREATE TABLE day_ov (
    date              DATE PRIMARY KEY,
    day               VARCHAR(13) GENERATED ALWAYS AS (
                          (ARRAY['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'])
                          [EXTRACT(DOW FROM date)::int + 1]
                      ) STORED,                      -- auto-filled from date
    starting_balance  NUMERIC(14,2) NOT NULL,
    overall_trend     trend_enum,
    volatility        volatility_enum,
    mp_state          mp_state_enum,
    news              VARCHAR(150),
    notes             TEXT,
    lesson            VARCHAR,                       -- moved here from day_sum (manual input)
    to_improve        VARCHAR                        -- moved here from day_sum (manual input)
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
    exit_actual    NUMERIC(14,4),
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
-- trade_log (VIEW) — computed values per trade. Same columns as Option A's
-- table, keyed by trade_info.id.
-- ---------------------------------------------------------------------------
CREATE VIEW trade_log AS
WITH base AS (
    SELECT
        t.id,
        d.starting_balance,
        t.entry_actual,
        t.size,
        -- Risk ($) = |planned entry - stop| * size
        ABS(t.entry_planned - t.sl) * t.size AS risk_amount,
        -- Planned R:R = |tp - planned entry| / |planned entry - stop|
        CASE WHEN ABS(t.entry_planned - t.sl) = 0 THEN NULL
             ELSE ABS(t.tp - t.entry_planned) / ABS(t.entry_planned - t.sl)
        END AS rr_planned,
        -- P&L ($) — direction-aware, net of fees; NULL until the trade is closed
        CASE
            WHEN t.entry_actual IS NULL OR t.exit_actual IS NULL THEN NULL
            WHEN t.direction = 'Long'  THEN (t.exit_actual - t.entry_actual) * t.size - COALESCE(t.fees,0)
            WHEN t.direction = 'Short' THEN (t.entry_actual - t.exit_actual) * t.size - COALESCE(t.fees,0)
        END AS pl_amount
    FROM trade_info t
    JOIN day_ov d ON d.date = t.trade_date
)
SELECT
    id,
    ROUND(risk_amount, 2)                                                         AS risk_amount,
    CASE WHEN starting_balance = 0 THEN NULL ELSE risk_amount / starting_balance END  AS risk_pct,
    rr_planned,
    ROUND(pl_amount, 2)                                                           AS pl_amount,
    CASE WHEN entry_actual IS NULL OR entry_actual * size = 0 THEN NULL
         ELSE pl_amount / (entry_actual * size) END                               AS pl_percent,
    CASE WHEN risk_amount IS NULL OR risk_amount = 0 THEN NULL
         ELSE pl_amount / risk_amount END                                         AS realized_r,
    CASE WHEN pl_amount IS NULL THEN NULL
         WHEN pl_amount > 0 THEN 'Win'
         WHEN pl_amount < 0 THEN 'Loss'
         ELSE 'Breakeven' END::VARCHAR(10)                                        AS outcome
FROM base;

-- ---------------------------------------------------------------------------
-- day_sum (VIEW) — one row per day in day_ov (days with no trades show zeros)
-- Also carries ending_balance, which Option A stores on day_ov.
-- ---------------------------------------------------------------------------
CREATE VIEW day_sum AS
WITH trades AS (
    SELECT ti.trade_date, ti.id, ti.followed_rules, tl.pl_amount, tl.outcome
    FROM trade_info ti
    JOIN trade_log tl ON tl.id = ti.id
),
ranked AS (
    SELECT trade_date, id, pl_amount,
           ROW_NUMBER() OVER (PARTITION BY trade_date ORDER BY pl_amount DESC, id) AS rn_best,
           ROW_NUMBER() OVER (PARTITION BY trade_date ORDER BY pl_amount ASC,  id) AS rn_worst
    FROM trades
    WHERE pl_amount IS NOT NULL
),
agg AS (
    SELECT
        trade_date,
        COUNT(*)::SMALLINT                                  AS total_trades,
        (COUNT(*) FILTER (WHERE outcome = 'Win'))::SMALLINT   AS wins,
        (COUNT(*) FILTER (WHERE outcome = 'Loss'))::SMALLINT  AS losses,
        COALESCE(SUM(pl_amount), 0)                          AS net_pl_amount,
        BOOL_OR(followed_rules = FALSE)                      AS broke_rules
    FROM trades
    GROUP BY trade_date
)
SELECT
    d.date,
    COALESCE(a.total_trades, 0::SMALLINT)                    AS total_trades,
    COALESCE(a.wins, 0::SMALLINT)                            AS wins,
    COALESCE(a.losses, 0::SMALLINT)                          AS losses,
    COALESCE(a.net_pl_amount, 0)                             AS net_pl_amount,
    CASE WHEN COALESCE(a.total_trades,0) = 0 THEN NULL
         ELSE ROUND(a.wins::NUMERIC / a.total_trades, 4) END AS win_rate,
    CASE WHEN d.starting_balance = 0 THEN NULL
         ELSE COALESCE(a.net_pl_amount, 0) / d.starting_balance END  AS net_pl_percent,
    best.pl_amount                                           AS best_trade_amount,
    best.id                                                  AS best_trade_id,
    worst.pl_amount                                          AS worst_trade_amount,
    worst.id                                                 AS worst_trade_id,
    CASE WHEN COALESCE(a.total_trades,0) = 0 THEN NULL
         WHEN a.broke_rules THEN 'No - see Trade Log'
         ELSE 'Yes' END::VARCHAR(100)                        AS followed_plan_ov,
    d.lesson,
    d.to_improve,
    d.starting_balance + COALESCE(a.net_pl_amount, 0)        AS ending_balance
FROM day_ov d
LEFT JOIN agg a        ON a.trade_date = d.date
LEFT JOIN ranked best  ON best.trade_date = d.date AND best.rn_best = 1
LEFT JOIN ranked worst ON worst.trade_date = d.date AND worst.rn_worst = 1;
