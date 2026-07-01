import axios from "axios";
import type {
  DataPoolInfo,
  DataPoolCode,
  DataPoolListResponse,
  DataPoolCodesResponse,
  AddCodeRequest,
} from "../types/datapool";

const PO_API_BASE_URL =
  import.meta.env.VITE_PO_API_URL || "http://localhost:9999";

const apiClient = axios.create({
  baseURL: PO_API_BASE_URL,
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
});

export interface ImportCSVRequest {
  poolName: string;
  csvPath: string;
  userName?: string;
  createID: string;
  codeColumn?: string;
  noteColumn?: string;
  note?: string;
}

export interface AddFromReaderRequest {
  poolName: string;
  code: string;
  batchID: string;
  note?: string;
}

export interface ImportCSVFromContentRequest {
  poolName: string;
  csvContent: string;
  createID: string;
  userName?: string;
  note?: string;
}

export const datapoolApi = {
  /** Get pools with stats */
  async getPoolsWithStats(): Promise<DataPoolInfo[]> {
    const response = await apiClient.get<DataPoolListResponse>("/api/datapool/pools/stats");
    if (!response.data.success) {
      throw new Error(response.data.message || "Failed to fetch pool stats");
    }
    return response.data.data || [];
  },

  /** Get codes in a pool (limited) */
  async getCodes(poolName: string, limit = 20): Promise<DataPoolCode[]> {
    const response = await apiClient.get<DataPoolCodesResponse>(
      `/api/datapool/${encodeURIComponent(poolName)}/codes?limit=${limit}`,
    );
    if (!response.data.success) {
      throw new Error(response.data.message || "Failed to fetch codes");
    }
    return response.data.data || [];
  },

  /** Search codes in a pool (like) */
  async searchCodes(poolName: string, keyword: string, limit = 50): Promise<DataPoolCode[]> {
    const response = await apiClient.get<DataPoolCodesResponse>(
      `/api/datapool/${encodeURIComponent(poolName)}/codes/search?q=${encodeURIComponent(keyword)}&limit=${limit}`,
    );
    if (!response.data.success) {
      throw new Error(response.data.message || "Failed to search codes");
    }
    return response.data.data || [];
  },

  /** Add single code manually */
  async addCode(request: AddCodeRequest): Promise<void> {
    await apiClient.post("/api/datapool/add", request);
  },

  /** Create a new pool */
  async createPool(poolName: string): Promise<void> {
    await apiClient.post("/api/datapool/pools", { poolName });
  },

  /** Import codes from CSV file (legacy - server path) */
  async importCSV(request: ImportCSVRequest): Promise<void> {
    await apiClient.post("/api/datapool/import-file", {
      poolName: request.poolName,
      csvPath: request.csvPath,
      userName: request.userName || "admin",
      createID: request.createID,
      codeColumn: request.codeColumn || "Code",
      noteColumn: request.noteColumn || "",
      note: request.note || "",
    });
  },

  /** Import codes from CSV content (client uploads file) */
  async importCSVFromContent(request: ImportCSVFromContentRequest): Promise<void> {
    await apiClient.post("/api/datapool/import-content", {
      poolName: request.poolName,
      csvContent: request.csvContent,
      createID: request.createID,
      userName: request.userName || "admin",
      note: request.note || "",
    });
  },

  /** Add code from reader/camera (auto marks as Used) */
  async addFromReader(request: AddFromReaderRequest): Promise<void> {
    await apiClient.post("/api/datapool/add-reader", {
      poolName: request.poolName,
      code: request.code,
      batchID: request.batchID,
      note: request.note || "",
    });
  },
};

export default datapoolApi;
