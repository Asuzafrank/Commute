// frontend/src/services/api/stations.service.js
import apiClient from "./client";

const stationsService = {
  /**
   * Search stations by name
   * @param {string} query - Search term (min 2 characters)
   * @param {number} limit - Max results (default 20)
   */
  async search(query, limit = 20) {
    if (!query || query.length < 2) {
      return { success: true, data: [] };
    }

    const response = await apiClient.get("/stations/search", {
      params: { q: query, limit },
    });
    return response.data;
  },

  /**
   * Get station by ID
   * @param {string} stopId - Station ID (e.g., "place-north")
   */
  async getStationById(stopId) {
    const response = await apiClient.get(`/stations/${stopId}`);
    return response.data;
  },
  async getNearby(lat,lon, radius = 1000, limit = 20) {
    const response = await apiClient.get("/stations/nearby", {
      params: { lat, lon, radius, limit },
    });
    return response.data;
  }
};

export default stationsService;
