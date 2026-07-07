import axios from "axios";
import type {
  AuthUser,
  CurrentUser,
  LoginRequest,
  LoginResponse,
  User,
  UsersListResponse,
  LoginHistoryEntry,
  HistoryResponse,
  CreateUserRequest,
  UpdateUserRequest,
} from "../types/auth";

const PO_API_BASE_URL =
  import.meta.env.VITE_PO_API_URL || "http://localhost:9999";

const authApiClient = axios.create({
  baseURL: PO_API_BASE_URL,
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
  withCredentials: true,
});

export const authApi = {
  async login(request: LoginRequest): Promise<LoginResponse> {
    const response = await authApiClient.post<LoginResponse>("/api/auth/login", request);
    return response.data;
  },

  async logout(): Promise<{ success: boolean; message?: string }> {
    const response = await authApiClient.post<{ success: boolean; message?: string }>("/api/auth/logout");
    return response.data;
  },

  async getCurrentUser(): Promise<{ success: boolean; user?: CurrentUser; message?: string }> {
    const response = await authApiClient.get<{ success: boolean; user?: CurrentUser; message?: string }>("/api/auth/me");
    return response.data;
  },

  async getUsers(): Promise<UsersListResponse> {
    const response = await authApiClient.get<UsersListResponse>("/api/auth/users");
    return response.data;
  },

  async createUser(request: CreateUserRequest): Promise<{ success: boolean; message?: string }> {
    const response = await authApiClient.post<{ success: boolean; message?: string }>("/api/auth/users", request);
    return response.data;
  },

  async updateUser(id: string, request: UpdateUserRequest): Promise<{ success: boolean; message?: string }> {
    const response = await authApiClient.put<{ success: boolean; message?: string }>(`/api/auth/users/${id}`, request);
    return response.data;
  },

  async deleteUser(id: string): Promise<{ success: boolean; message?: string }> {
    const response = await authApiClient.delete<{ success: boolean; message?: string }>(`/api/auth/users/${id}`);
    return response.data;
  },

  async getLoginHistory(): Promise<HistoryResponse> {
    const response = await authApiClient.get<HistoryResponse>("/api/auth/history");
    return response.data;
  },
};

export function hasPermission(role: string, permission: string): boolean {
const permissions: Record<string, string[]> = {
  sadmin: ["view", "add", "edit", "import", "manage_pools", "manage_users", "view_history"],
  administrator: ["view", "add", "edit", "import", "manage_pools", "view_history"],
  operator: ["view", "add", "edit", "import"],
  viewer: ["view"],
};
  return permissions[role]?.includes(permission) ?? false;
}

export function canManageUsers(role: string): boolean {
  return hasPermission(role, "manage_users");
}

export function canViewHistory(role: string): boolean {
  return hasPermission(role, "view_history");
}

export function canManagePools(role: string): boolean {
  return hasPermission(role, "manage_pools");
}
