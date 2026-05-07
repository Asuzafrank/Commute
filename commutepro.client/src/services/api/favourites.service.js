// frontend/src/services/api/favourites.service.js
import apiClient from "./client";

const favouritesService = {
  /**
   * Get all favourite stations for current user
   */
  async getFavourites() {
    try {
      const response = await apiClient.get("/favourites");
      return response.data;
    } catch (error) {
      // If 404 (no favourites), return empty list instead of error
      if (error.response?.status === 404) {
        console.log("No favourites found, returning empty list");
        return { success: true, data: [] };
      }
      throw error;
    }
  },

  /**
   * Add a station to favourites
   * @param {string} stopId - Station ID to add
   */
  async addFavourite(stopId) {
    const response = await apiClient.post("/favourites", { stopId });
    return response.data;
  },

  /**
   * Remove a station from favourites
   * @param {string} favouriteId - Favourite ID (GUID)
   */
  async removeFavourite(favouriteId) {
    const response = await apiClient.delete(`/favourites/${favouriteId}`);
    return response.data;
  },

  /**
   * Reorder favourites
   * @param {Object} newOrder - Dictionary { favouriteId: sortOrder }
   */
  async reorderFavourites(newOrder) {
    const response = await apiClient.post("/favourites/reorder", { newOrder });
    return response.data;
  },

  /**
   * Check if a station is in favourites
   * @param {string} stopId - Station ID to check
   */
  async isFavourite(stopId) {
    try {
      const response = await apiClient.get(`/favourites/check/${stopId}`);
      return response.data;
    } catch (error) {
      // If 404, station is not a favourite
      if (error.response?.status === 404) {
        return { success: true, data: false };
      }
      throw error;
    }
  },
};

export default favouritesService;
