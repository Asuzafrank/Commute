// frontend/src/services/api/trips.service.js
import apiClient from "./client";

const tripsService = {
  async getTripDetails(tripId) {
    const response = await apiClient.get(`/trips/${tripId}`);
    return response.data;
  },

  async planDirectTrip(fromStopId, toStopId, departureTime) {
   const response = await apiClient.get("/trips/plan-direct", {
     params: {
       from: fromStopId,
       to: toStopId,
       departureTime: departureTime,
     },
   });
   return response.data;
 },
};

export default tripsService;
