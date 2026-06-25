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

export const datapoolApi = {
  /** List all pools */
  async listPools(): Promise<DataPoolInfo[]> {
    const response = await apiClient.get<DataPoolListResponse>("/api/datapool/pools");
    if (!response.data.success) {
      throw new Error(response.data.message || "Failed to fetch pools");
    }
    return response.data.data || [];
  },

  /** Get codes in a pool */
  async getCodes(poolName: string): Promise<DataPoolCode[]> {
    const response = await apiClient.get<DataPoolCodesResponse>(
      `/api/datapool/${encodeURIComponent(poolName)}/codes`,
    );
    if (!response.data.success) {
      throw new Error(response.data.message || "Failed to fetch codes");
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

  /** Import codes from CSV file */
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
