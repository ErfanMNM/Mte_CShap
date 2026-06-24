import axios from "axios";
import type {
  HealthResponse,
  POInfo,
  POListItem,
  CreatePORequest,
  CreatePOResponse,
  PODetailResponse,
  POListResponse,
} from "../types/po";

const PO_API_BASE_URL =
  import.meta.env.VITE_PO_API_URL || "http://localhost:9999";

const apiClient = axios.create({
  baseURL: PO_API_BASE_URL,
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
});

export const poApi = {
  async checkHealth(): Promise<HealthResponse> {
    const response = await apiClient.get<HealthResponse>("/api/health");
    return response.data;
  },

  async getAllPOs(): Promise<POListItem[]> {
    const response = await apiClient.get<POListResponse>("/api/po/list/all");
    if (!response.data.success) {
      throw new Error(response.data.message || "Failed to fetch PO list");
    }
    return response.data.data || [];
  },

  async getPO(orderNo: string): Promise<POInfo> {
    const response = await apiClient.get<PODetailResponse>(
      `/api/po/${encodeURIComponent(orderNo)}`,
    );
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "PO not found");
    }
    return response.data.data;
  },

  async createPO(poData: CreatePORequest): Promise<CreatePOResponse> {
    const response = await apiClient.post<CreatePOResponse>("/api/po", poData);
    return response.data;
  },

  /** Check if a PO can be deleted (no codes used). */
  async canDeletePO(orderNo: string): Promise<CanDeleteResponse> {
    const response = await apiClient.get<CanDeleteResponse>(
      `/api/po/${encodeURIComponent(orderNo)}/can-delete`,
    );
    return response.data;
  },

  /** Delete a PO. Returns success=false if codes have been used. */
  async deletePO(orderNo: string, userName = "Admin"): Promise<DeletePOResponse> {
    const response = await apiClient.delete<DeletePOResponse>(
      `/api/po/${encodeURIComponent(orderNo)}`,
      { headers: { "X-User-Name": userName } },
    );
    return response.data;
  },
};

export interface CanDeleteResponse {
  canDelete: boolean;
  reason: string;
  usedCodesCount: number;
  totalCodesCount: number;
}

export interface DeletePOResponse {
  success: boolean;
  message: string;
  canDelete: boolean;
  reason: string;
}

export default poApi;
