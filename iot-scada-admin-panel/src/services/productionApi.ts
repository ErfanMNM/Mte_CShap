import axios from "axios";
import type {
  StartProductionRequest,
  ProductionControlResponse,
  ProductionStatusResponse,
} from "../types/production";

const PO_API_BASE_URL =
  import.meta.env.VITE_PO_API_URL || "http://localhost:9999";

const apiClient = axios.create({
  baseURL: PO_API_BASE_URL,
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
});

export const productionApi = {
  /** Start production with a PO */
  async startProduction(
    request: StartProductionRequest
  ): Promise<ProductionControlResponse> {
    const response = await apiClient.post<ProductionControlResponse>(
      "/api/production/start",
      request
    );
    return response.data;
  },

  /** Stop production */
  async stopProduction(userName = "Frontend"): Promise<ProductionControlResponse> {
    const response = await apiClient.post<ProductionControlResponse>(
      "/api/production/stop",
      {},
      { headers: { "X-User-Name": userName } }
    );
    return response.data;
  },

  /** Reset production (clear PO) */
  async resetProduction(userName = "Frontend"): Promise<ProductionControlResponse> {
    const response = await apiClient.post<ProductionControlResponse>(
      "/api/production/reset",
      {},
      { headers: { "X-User-Name": userName } }
    );
    return response.data;
  },

  /** Get current production status */
  async getStatus(): Promise<ProductionStatusResponse> {
    const response = await apiClient.get<ProductionStatusResponse>(
      "/api/production/status"
    );
    return response.data;
  },
};

export default productionApi;
