// frontend/src/services/api/auth.service.js
import apiClient from "./client";

const authService = {
  /**
   * Register a new user
   * @param {Object} userData - { email, userName, password }
   */
  async register(userData) {
    const response = await apiClient.post("/Auth/register", userData);
    return response.data;
  },

  /**
   * Login user
   * @param {Object} credentials - { email, password }
   */
  async login(credentials) {
    const response = await apiClient.post("/Auth/login", credentials);

    if (response.data.success && response.data.data) {
      const { token, refreshToken, userId, email, userName, expiresAt } = response.data.data;

      // Store tokens
      localStorage.setItem("accessToken", token);
      localStorage.setItem("refreshToken", refreshToken);
      localStorage.setItem("userId", userId);
      localStorage.setItem("userEmail", email);
      localStorage.setItem("userName", userName);
      localStorage.setItem("tokenExpiresAt", expiresAt);
    }

    return response.data;
  },

  /**
   * Logout user
   */
  logout() {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("userId");
    localStorage.removeItem("userEmail");
    localStorage.removeItem("userName");
    localStorage.removeItem("tokenExpiresAt");
  },

  /**
   * Check if user is logged in
   */
  isAuthenticated() {
    const token = localStorage.getItem("accessToken");
    const expiresAt = localStorage.getItem("tokenExpiresAt");

    if (!token) return false;

    // Check if token expired
    if (expiresAt && new Date(expiresAt) < new Date()) {
      this.logout();
      return false;
    }

    return true;
  },

  /**
   * Get current user info
   */
  getCurrentUser() {
    return {
      userId: localStorage.getItem("userId"),
      email: localStorage.getItem("userEmail"),
      userName: localStorage.getItem("userName"),
      isAuthenticated: this.isAuthenticated(),
    };
  },
};

export default authService;
