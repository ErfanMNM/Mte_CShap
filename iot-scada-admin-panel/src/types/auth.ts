// TypeScript interfaces for Auth API

export type UserRole = "SAdmin" | "Administrator" | "Operator" | "Viewer";

export interface AuthUser {
  id: string;
  username: string;
  displayName: string;
  role: UserRole;
}

export interface CurrentUser extends AuthUser {
  sessionId: string;
  userId: string;
  expiresAt: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  user?: AuthUser;
  message?: string;
}

export interface User extends AuthUser {
  isActive: boolean;
  createdAt: string;
  createdBy?: string;
}

export interface LoginHistoryEntry {
  id: string;
  userId?: string;
  username: string;
  loginTime: string;
  logoutTime?: string;
  ipAddress?: string;
  userAgent?: string;
  success: boolean;
  failureReason?: string;
}

export interface CreateUserRequest {
  username: string;
  password: string;
  displayName?: string;
  role: UserRole;
}

export interface UpdateUserRequest {
  password?: string;
  displayName?: string;
  role?: UserRole;
  isActive?: boolean;
}

export interface UsersListResponse {
  success: boolean;
  count?: number;
  data?: User[];
  message?: string;
}

export interface HistoryResponse {
  success: boolean;
  count?: number;
  data?: LoginHistoryEntry[];
  message?: string;
}
