// PLC API — recipe DB + read/write from Omron PLC via GProject backend.
// Endpoints registered in GProject/GProjectApiServer.cs (PLC RECIPE region).

import axios from "axios";
import type { PLCRecipe } from "../types/plc";

const PO_API_BASE_URL =
  import.meta.env.VITE_PO_API_URL || "http://localhost:9999";

const apiClient = axios.create({
  baseURL: PO_API_BASE_URL,
  timeout: 15000,
  headers: {
    "Content-Type": "application/json",
  },
});

export interface ApiOk<T> {
  success: boolean;
  message?: string;
  data?: T | null;
  count?: number;
}

export interface RecipeFromPlc {
  delayCamera: number;
  delayReject: number;
  rejectStreng: number;
}

export const plcApi = {
  /** GET /api/plc/recipe/active */
  async getActiveRecipe(): Promise<PLCRecipe | null> {
    const r = await apiClient.get<ApiOk<PLCRecipe>>("/api/plc/recipe/active");
    if (!r.data.success) throw new Error(r.data.message || "Failed");
    return r.data.data ?? null;
  },

  /** GET /api/plc/recipe/list */
  async getAllRecipes(): Promise<PLCRecipe[]> {
    const r = await apiClient.get<ApiOk<PLCRecipe[]>>("/api/plc/recipe/list");
    if (!r.data.success) throw new Error(r.data.message || "Failed");
    return r.data.data ?? [];
  },

  /** POST /api/plc/recipe/save */
  async saveRecipe(recipe: PLCRecipe): Promise<ApiOk<PLCRecipe>> {
    const r = await apiClient.post<ApiOk<PLCRecipe>>(
      "/api/plc/recipe/save",
      recipe,
    );
    if (!r.data.success) throw new Error(r.data.message || "Save failed");
    return r.data;
  },

  /** POST /api/plc/recipe/active body {id} */
  async setActiveRecipe(id: number): Promise<ApiOk<PLCRecipe>> {
    const r = await apiClient.post<ApiOk<PLCRecipe>>(
      "/api/plc/recipe/active",
      { id },
    );
    if (!r.data.success) throw new Error(r.data.message || "Set active failed");
    return r.data;
  },

  /** DELETE /api/plc/recipe/{id} */
  async deleteRecipe(id: number): Promise<ApiOk<null>> {
    const r = await apiClient.delete<ApiOk<null>>(
      `/api/plc/recipe/${id}`,
    );
    if (!r.data.success) throw new Error(r.data.message || "Delete failed");
    return r.data;
  },

  /** GET /api/plc/recipe/from-plc — đọc 3 int32 trực tiếp từ PLC */
  async getRecipeFromPlc(): Promise<RecipeFromPlc | null> {
    const r = await apiClient.get<ApiOk<RecipeFromPlc>>(
      "/api/plc/recipe/from-plc",
    );
    if (!r.data.success) {
      throw new Error(r.data.message || "PLC read failed");
    }
    return r.data.data ?? null;
  },
};

export default plcApi;
