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
};

export default datapoolApi;
