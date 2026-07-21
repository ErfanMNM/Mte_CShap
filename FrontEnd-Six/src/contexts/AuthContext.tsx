import React, { createContext, useContext, useState, useEffect, useCallback, type ReactNode } from "react";
import { authApi } from "../services/authApi";
import type { CurrentUser } from "../types/auth";

interface AuthContextType {
  user: CurrentUser | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (username: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  checkAuth: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const AUTH_STORAGE_KEY = "gauth_user";
const LOGIN_TS_KEY = "gauth_login_ts";

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const checkAuth = useCallback(async (productionState?: string) => {
    try {
      if (productionState === "NeedLogin") {
        setUser(null);
        localStorage.removeItem(AUTH_STORAGE_KEY);
        setIsLoading(false);
        return;
      }

      const response = await authApi.getCurrentUser();
      if (response.success && response.user) {
        setUser(response.user);
      } else {
        setUser(null);
      }
    } catch {
      setUser(null);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    checkAuth();
  }, [checkAuth]);

  const login = async (username: string, password: string) => {
    const response = await authApi.login({ username, password });
    if (!response.success || !response.user) {
      throw new Error(response.message || "Login failed");
    }
    
    const currentUser: CurrentUser = {
      ...response.user,
      sessionId: "",
      userId: response.user.id,
      expiresAt: new Date(Date.now() + 8 * 60 * 60 * 1000).toISOString(),
    };
    
    setUser(currentUser);
    localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(currentUser));
    localStorage.setItem(LOGIN_TS_KEY, String(Date.now()));
  };

  const logout = async () => {
    try {
      await authApi.logout();
    } finally {
      setUser(null);
      localStorage.removeItem(AUTH_STORAGE_KEY);
      localStorage.removeItem(LOGIN_TS_KEY);
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isLoading,
        isAuthenticated: !!user,
        login,
        logout,
        checkAuth,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
