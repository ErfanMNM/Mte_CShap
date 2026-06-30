import axios from "axios";
import type {
  HealthResponse,
  POInfo,
  POListItem,
  CreatePORequest,
  CreatePOResponse,
  PODetailResponse,
  POListResponse,
  POCode,
  POCodeListResponse,
  POCarton,
  POCartonListResponse,
  ProductionStatusResponse,
  ApiResponse,
} from "../types/po";

const PO_API_BASE_URL =
  import.meta.env.VITE_PO_API_URL || "http://localhost:9999";

const apiClient = axios.create({
  baseURL: PO_API_BASE_URL,
  timeout: 15000,
  headers: {
    "Content-Type": "application/json",
  },
});

export const poApi = {
  // Health check
  async checkHealth(): Promise<HealthResponse> {
    const response = await apiClient.get<HealthResponse>("/api/health");
    return response.data;
  },

  // PO List
  async getAllPOs(): Promise<POListItem[]> {
    const response = await apiClient.get<POListResponse>("/api/po/list");
    if (!response.data.success) {
      throw new Error(response.data.message || "Failed to fetch PO list");
    }
    return response.data.data || [];
  },

  // PO Detail
  async getPO(orderNo: string): Promise<POInfo> {
    const response = await apiClient.get<PODetailResponse>(
      `/api/po/${encodeURIComponent(orderNo)}`,
    );
    if (!response.data.success || !response.data.data) {
      throw new Error(response.data.message || "PO not found");
    }
    return response.data.data;
  },

  // Create PO
  async createPO(poData: CreatePORequest): Promise<CreatePOResponse> {
    const response = await apiClient.post<CreatePOResponse>("/api/po", poData);
    return response.data;
  },

  // Update PO
  async updatePO(orderNo: string, poData: Partial<CreatePORequest>): Promise<ApiResponse> {
    const response = await apiClient.put<ApiResponse>(
      `/api/po/${encodeURIComponent(orderNo)}`,
      poData,
    );
    return response.data;
  },

  // Check if PO can be deleted
  async canDeletePO(orderNo: string): Promise<CanDeleteResponse> {
    const response = await apiClient.get<CanDeleteResponse>(
      `/api/po/${encodeURIComponent(orderNo)}/can-delete`,
    );
    return response.data;
  },

  // Delete PO
  async deletePO(orderNo: string, userName = "Frontend"): Promise<DeletePOResponse> {
    const response = await apiClient.delete<DeletePOResponse>(
      `/api/po/${encodeURIComponent(orderNo)}`,
      { data: { userName } },
    );
    return response.data;
  },

  // Codes
  async getCodes(orderNo: string, status?: number, cartonCode?: string, limit = 100): Promise<POCode[]> {
    const params = new URLSearchParams();
    if (status !== undefined) params.append("status", status.toString());
    if (cartonCode) params.append("cartonCode", cartonCode);
    params.append("limit", limit.toString());

    const response = await apiClient.get<POCodeListResponse>(
      `/api/po/${encodeURIComponent(orderNo)}/codes?${params.toString()}`,
    );
    if (!response.data.success) {
      throw new Error(response.data.message || "Failed to fetch codes");
    }
    return response.data.data || [];
  },

  async activateCode(orderNo: string, code: string, activateUser = "Frontend"): Promise<ApiResponse> {
    const response = await apiClient.post<ApiResponse>(
      `/api/po/${encodeURIComponent(orderNo)}/activate`,
      { code, activateUser, activateDate: new Date().toISOString() },
    );
    return response.data;
  },

  async packCode(orderNo: string, code: string, cartonCode: string, packingUser = "Frontend"): Promise<ApiResponse> {
    const response = await apiClient.post<ApiResponse>(
      `/api/po/${encodeURIComponent(orderNo)}/pack`,
      { code, cartonCode, packingUser, packingDate: new Date().toISOString() },
    );
    return response.data;
  },

  // Cartons
  async getCartons(orderNo: string): Promise<POCarton[]> {
    const response = await apiClient.get<POCartonListResponse>(
      `/api/po/${encodeURIComponent(orderNo)}/cartons`,
    );
    if (!response.data.success) {
      throw new Error(response.data.message || "Failed to fetch cartons");
    }
    return response.data.data || [];
  },

  async startCarton(orderNo: string, cartonId: number, activateUser = "Frontend"): Promise<ApiResponse> {
    const response = await apiClient.post<ApiResponse>(
      `/api/po/${encodeURIComponent(orderNo)}/cartons/start`,
      { cartonId, activateUser },
    );
    return response.data;
  },

  async completeCarton(orderNo: string, cartonId: number, activateUser = "Frontend"): Promise<ApiResponse> {
    const response = await apiClient.post<ApiResponse>(
      `/api/po/${encodeURIComponent(orderNo)}/cartons/complete`,
      { cartonId, activateUser },
    );
    return response.data;
  },

  // Production Status
  async getProductionStatus(): Promise<ProductionStatusResponse> {
    const response = await apiClient.get<ProductionStatusResponse>("/api/production/status");
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
