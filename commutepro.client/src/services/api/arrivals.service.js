// frontend/src/services/api/arrivals.service.js
import apiClient from "./client";

const arrivalsService = {
  /**
   * Get real-time arrivals for a station
   * @param {string} stopId - Station ID
   */
  async getArrivals(stopId) {
    const response = await apiClient.get(`/Arrivals/${stopId}`);
    return response.data; // Returns raw ArrivalsResponseDto (no wrapper)
  },

  /**
   * Get arrivals for multiple stations (batch)
   * @param {string[]} stopIds - Array of station IDs
   */
  async getBatchArrivals(stopIds) {
    const response = await apiClient.post("/Arrivals/batch", { stopIds });
    return response.data;
  },

  /**
   * Get arrivals for user's favourite stations (requires auth)
   */
  async getFavouriteArrivals() {
    const response = await apiClient.get("/Arrivals/favourites");
    return response.data;
  },
};

export default arrivalsService;
