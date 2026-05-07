// frontend/src/stores/auth.store.js
import { defineStore } from "pinia";
import authService from "@/services/api/auth.service";

export const useAuthStore = defineStore("auth", {
  state: () => ({
    user: {
      userId: localStorage.getItem("userId") || null,
      email: localStorage.getItem("userEmail") || null,
      userName: localStorage.getItem("userName") || null,
    },
    isAuthenticated: authService.isAuthenticated(),
    loading: false,
    error: null,
  }),

  actions: {
    async register(userData) {
      this.loading = true;
      this.error = null;

      try {
        const response = await authService.register(userData);
        if (response.success) {
          return response;
        }
        this.error = response.message;
        return response;
      } catch (error) {
        this.error = error.response?.data?.message || "Registration failed";
        throw error;
      } finally {
        this.loading = false;
      }
    },

    async login(credentials) {
      this.loading = true;
      this.error = null;

      try {
        const response = await authService.login(credentials);
        if (response.success) {
          this.user = {
            userId: localStorage.getItem("userId"),
            email: localStorage.getItem("userEmail"),
            userName: localStorage.getItem("userName"),
          };
          this.isAuthenticated = true;
        } else {
          this.error = response.message;
        }
        return response;
      } catch (error) {
        this.error = error.response?.data?.message || "Login failed";
        throw error;
      } finally {
        this.loading = false;
      }
    },

    logout() {
      authService.logout();
      this.user = { userId: null, email: null, userName: null };
      this.isAuthenticated = false;
      this.error = null;
    },
  },
});
