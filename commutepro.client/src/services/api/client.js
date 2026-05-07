// frontend/src/services/api/client.js
import axios from "axios";

// Base configuration
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "https://localhost:7213/api";

// Create axios instance
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
    Accept: "application/json",
  },
  timeout: 30000, // 30 seconds
  withCredentials: true,
});

// Request interceptor - adds auth token to every request
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("accessToken");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  },
);

// Response interceptor - handles common errors
apiClient.interceptors.response.use(
  (response) => {
    return response;
  },
  async (error) => {
    const originalRequest = error.config;

    // Handle 401 Unauthorized - token expired
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      // Clear expired token
      localStorage.removeItem("accessToken");
      localStorage.removeItem("refreshToken");

      // Redirect to login page
      window.location.href = "/login";
    }

    // Handle 404 Not Found
    if (error.response?.status === 404) {
      console.error("Resource not found:", error.config.url);
    }

    // Handle 500 Server Error
    if (error.response?.status >= 500) {
      console.error("Server error:", error.response?.data?.message || "Internal server error");
    }

    return Promise.reject(error);
  },
);

export default apiClient;
