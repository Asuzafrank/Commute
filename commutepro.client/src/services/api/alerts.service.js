// frontend/src/services/api/alerts.service.js
import apiClient from "./client";

const alertsService = {
  /**
   * Get all active alerts
   */
  async getAllAlerts() {
    const response = await apiClient.get("/alerts");
    return response.data;
  },

  /**
   * Get alerts by route
   * @param {string} routeId - Route ID
   */
  async getAlertsByRoute(routeId) {
    const response = await apiClient.get(`/alerts/route/${routeId}`);
    return response.data;
  },

  /**
   * Get alerts by station
   * @param {string} stopId - Station ID
   */
  async getAlertsByStation(stopId) {
    const response = await apiClient.get(`/alerts/station/${stopId}`);
    return response.data;
  },

  /**
   * Get alerts for user's favourites (requires auth)
   */
  async getFavouriteAlerts() {
    const response = await apiClient.get("/alerts/favourites");
    return response.data;
  },

  /**
   * Get alert by ID
   * @param {string} alertId - Alert ID
   */
  async getAlertById(alertId) {
    const response = await apiClient.get(`/alerts/${alertId}`);
    return response.data;
  },
};

export default alertsService;
