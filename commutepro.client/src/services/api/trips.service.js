// frontend/src/services/api/trips.service.js
import apiClient from "./client";

const tripsService = {
  async getTripDetails(tripId) {
    const response = await apiClient.get(`/trips/${tripId}`);
    return response.data;
  },
};

export default tripsService;
