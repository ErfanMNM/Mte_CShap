import React, { useState, useEffect } from "react";
import { useAuth } from "../contexts/AuthContext";

const LAST_USERNAME_KEY = "gauth_last_username";

export function LoginScreen() {
  const { login } = useAuth();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    const savedUsername = localStorage.getItem(LAST_USERNAME_KEY);
    if (savedUsername) {
      setUsername(savedUsername);
    }
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setIsLoading(true);

    try {
      await login(username, password);
      localStorage.setItem(LAST_USERNAME_KEY, username);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Login failed");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div style={{
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      minHeight: "100vh",
      background: "linear-gradient(135deg, #1a1a2e 0%, #16213e 100%)",
      fontFamily: "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif",
    }}>
      <div style={{
        background: "white",
        padding: "40px",
        borderRadius: "12px",
        boxShadow: "0 10px 40px rgba(0,0,0,0.3)",
        width: "100%",
        maxWidth: "400px",
      }}>
        <div style={{ textAlign: "center", marginBottom: "32px" }}>
          <h1 style={{
            fontSize: "28px",
            fontWeight: 600,
            color: "#1a1a2e",
            margin: "0 0 8px 0",
          }}>GProject</h1>
          <p style={{
            fontSize: "14px",
            color: "#666",
            margin: 0,
          }}>Sign in to continue</p>
        </div>

        <form onSubmit={handleSubmit}>
          <div style={{ marginBottom: "20px" }}>
            <label style={{
              display: "block",
              fontSize: "14px",
              fontWeight: 500,
              color: "#333",
              marginBottom: "6px",
            }}>
              Username
            </label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
              autoFocus
              style={{
                width: "100%",
                padding: "12px 14px",
                fontSize: "15px",
                border: "1px solid #ddd",
                borderRadius: "8px",
                outline: "none",
                transition: "border-color 0.2s",
                boxSizing: "border-box",
              }}
              onFocus={(e) => e.target.style.borderColor = "#4a90d9"}
              onBlur={(e) => e.target.style.borderColor = "#ddd"}
            />
          </div>

          <div style={{ marginBottom: "24px" }}>
            <label style={{
              display: "block",
              fontSize: "14px",
              fontWeight: 500,
              color: "#333",
              marginBottom: "6px",
            }}>
              Password
            </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              style={{
                width: "100%",
                padding: "12px 14px",
                fontSize: "15px",
                border: "1px solid #ddd",
                borderRadius: "8px",
                outline: "none",
                transition: "border-color 0.2s",
                boxSizing: "border-box",
              }}
              onFocus={(e) => e.target.style.borderColor = "#4a90d9"}
              onBlur={(e) => e.target.style.borderColor = "#ddd"}
            />
          </div>

          {error && (
            <div style={{
              padding: "12px",
              background: "#fee",
              border: "1px solid #fcc",
              borderRadius: "8px",
              color: "#c33",
              fontSize: "14px",
              marginBottom: "20px",
            }}>
              {error}
            </div>
          )}

          <button
            type="submit"
            disabled={isLoading}
            style={{
              width: "100%",
              padding: "14px",
              fontSize: "16px",
              fontWeight: 600,
              color: "white",
              background: isLoading ? "#9ac" : "#4a90d9",
              border: "none",
              borderRadius: "8px",
              cursor: isLoading ? "not-allowed" : "pointer",
              transition: "background 0.2s",
            }}
            onMouseOver={(e) => !isLoading && (e.target.style.background = "#3a80c9")}
            onMouseOut={(e) => !isLoading && (e.target.style.background = "#4a90d9")}
          >
            {isLoading ? "Signing in..." : "Sign In"}
          </button>
        </form>

        <div style={{
          marginTop: "24px",
          padding: "16px",
          background: "#f8f9fa",
          borderRadius: "8px",
          fontSize: "12px",
          color: "#666",
        }}>
          <strong>Demo Accounts:</strong>
          <div style={{ marginTop: "8px" }}>
            <div>sadmin / SAdmin@123 (Super Admin)</div>
            <div>admin / Admin@123 (Administrator)</div>
            <div>operator / Operator@123 (Operator)</div>
            <div>viewer / Viewer@123 (Viewer)</div>
          </div>
        </div>
      </div>
    </div>
  );
}
