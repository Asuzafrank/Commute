// frontend/src/services/api/vehicles.service.js
import apiClient from "./client";

const vehiclesService = {
  /**
   * Get all vehicle positions
   */
  async getVehiclePositions() {
    const response = await apiClient.get("/vehicles");
    return response.data;
  },
};

export default vehiclesService;
