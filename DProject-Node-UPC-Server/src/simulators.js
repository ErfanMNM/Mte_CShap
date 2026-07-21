// Simulator factory. Each simulator returns a closure that produces the next value
// given an elapsed time (ms) and a mutable state object.

function clamp(v, lo, hi) {
  if (v < lo) return lo;
  if (v > hi) return hi;
  return v;
}

export function createSimulator(config) {
  const kind = (config.simulator || "static").toLowerCase();

  switch (kind) {
    case "static":
      return () => ({
        value: config.value,
        done: false,
      });

    case "counter": {
      const step = Number.isFinite(config.step) ? config.step : 1;
      const state = { count: Number.isFinite(config.value) ? config.value : 0 };
      return () => {
        state.count += step;
        // wrap Int32 if step is integer and not intentionally float
        if (Number.isInteger(step)) {
          state.count = state.count | 0;
        }
        return { value: state.count, done: false };
      };
    }

    case "sine": {
      const min = Number(config.min ?? 0);
      const max = Number(config.max ?? 100);
      const period = Math.max(1, Number(config.periodMs ?? 1000));
      const amplitude = (max - min) / 2;
      const offset = (max + min) / 2;
      const t0 = Date.now();
      return () => {
        const t = Date.now() - t0;
        const phase = (2 * Math.PI * t) / period;
        const value = offset + amplitude * Math.sin(phase);
        return { value, done: false };
      };
    }

    case "random": {
      const min = Number(config.min ?? 0);
      const max = Number(config.max ?? 100);
      return () => ({
        value: min + Math.random() * (max - min),
        done: false,
      });
    }

    case "ramp": {
      const min = Number(config.min ?? 0);
      const max = Number(config.max ?? 100);
      const period = Math.max(1, Number(config.periodMs ?? 1000));
      // Increment per tick: covers full range in `period` ms
      const incPerTick = (max - min) / period;
      const state = { v: min };
      return () => {
        state.v += incPerTick;
        if (state.v >= max) state.v = min;
        return { value: state.v, done: false };
      };
    }

    default:
      // Unknown simulator -> treat as static of initial value
      return () => ({ value: config.value, done: false });
  }
}

export function needsTimer(config) {
  const kind = (config.simulator || "static").toLowerCase();
  return kind !== "static";
}

export function tickIntervalMs(config) {
  const ms = Number(config.periodMs);
  if (!Number.isFinite(ms) || ms <= 0) return 1000;
  return Math.floor(ms);
}

export function clampInt32(v) {
  // Node-side helper to keep values inside Int32 range when caller writes
  return clamp(Math.trunc(v), -2147483648, 2147483647);
}
