export type LogLevel =
  | "Verbose"
  | "Debug"
  | "Information"
  | "Warning"
  | "Error"
  | "Fatal";

export interface LogEntry {
  id: number;
  timestamp: string;
  level: string;
  levelName: string;
  message: string;
  messageTemplate?: string | null;
  exception?: string | null;
  properties?: string | null;
  sourceContext?: string | null;
  machineName?: string | null;
  threadId?: number | null;
}

export interface LogsPage {
  items: LogEntry[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNext: boolean;
  hasPrev: boolean;
}

export interface LogsApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

export interface LogDetailResponse {
  success: boolean;
  message: string;
  data: LogEntry;
}

export interface LogsQueryParams {
  from?: string;
  to?: string;
  level?: string;
  tag?: string;
  q?: string;
  page?: number;
  pageSize?: number;
  sort?: "asc" | "desc";
}

export function parseLogProperties(entry: LogEntry): Record<string, unknown> | null {
  if (!entry.properties) return null;
  try {
    const obj = JSON.parse(entry.properties);
    return obj && typeof obj === "object" ? (obj as Record<string, unknown>) : null;
  } catch {
    return null;
  }
}

export function getLogProperty(entry: LogEntry, key: string): unknown {
  const obj = parseLogProperties(entry);
  return obj ? obj[key] : undefined;
}