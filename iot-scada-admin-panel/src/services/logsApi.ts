import axios from "axios";
import type {
  LogEntry,
  LogsApiResponse,
  LogsPage,
  LogsQueryParams,
  LogDetailResponse,
} from "../types/logs";

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? "http://localhost:9999";

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 20000,
  headers: {
    "Content-Type": "application/json",
  },
  withCredentials: true,
});

function toIsoUtc(local: string | undefined): string | undefined {
  if (!local) return undefined;
  const d = new Date(local);
  if (Number.isNaN(d.getTime())) return undefined;
  return d.toISOString();
}

export const logsApi = {
  async query(params: LogsQueryParams = {}): Promise<LogsPage> {
    const search = new URLSearchParams();
    const fromIso = toIsoUtc(params.from);
    const toIso = toIsoUtc(params.to);
    if (fromIso) search.set("from", fromIso);
    if (toIso) search.set("to", toIso);
    if (params.level) search.set("level", params.level);
    if (params.tag) search.set("tag", params.tag);
    if (params.q) search.set("q", params.q);
    if (params.page && params.page > 0) search.set("page", String(params.page));
    if (params.pageSize && params.pageSize > 0)
      search.set("pageSize", String(Math.min(params.pageSize, 500)));
    search.set("sort", params.sort ?? "desc");

    const qs = search.toString();
    const url = qs ? `/api/logs?${qs}` : "/api/logs";
    const response = await apiClient.get<LogsApiResponse<LogsPage>>(url);
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Failed to fetch logs");
    }
    return response.data.data;
  },

  async getLevels(): Promise<string[]> {
    const response = await apiClient.get<LogsApiResponse<string[]>>(
      "/api/logs/levels",
    );
    if (!response.data.success) {
      throw new Error(response.data.message || "Failed to fetch log levels");
    }
    return response.data.data ?? [];
  },

  async getTags(): Promise<string[]> {
    const response = await apiClient.get<LogsApiResponse<string[]>>(
      "/api/logs/tags",
    );
    if (!response.data.success) {
      throw new Error(response.data.message || "Failed to fetch log tags");
    }
    return response.data.data ?? [];
  },

  async getDetail(id: number): Promise<LogEntry> {
    const response = await apiClient.get<LogDetailResponse>(
      `/api/logs/${id}`,
    );
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "Log entry not found");
    }
    return response.data.data;
  },
};

export default logsApi;