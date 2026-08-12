<!-- frontend/src/views/NearbyStationsView.vue -->
<template>
  <div class="min-h-screen" style="background: #243540">
    <!-- Header -->
    <div
      class="sticky top-0 z-40 px-5 pt-5 pb-4"
      style="
        background: rgba(28, 42, 52, 0.97);
        backdrop-filter: blur(20px);
        border-bottom: 1px solid rgba(255, 255, 255, 0.07);
      "
    >
      <div class="flex items-center gap-3">
        <button
          @click="goBack"
          class="w-9 h-9 rounded-lg flex items-center justify-center cursor-pointer transition-all hover:bg-white/5"
          style="background: rgba(255, 255, 255, 0.05); border: none"
        >
          <ChevronLeft size="20" color="rgba(255,255,255,0.75)" />
        </button>
        <h1 class="text-white text-xl font-bold font-manrope">Nearby Stations</h1>
      </div>
    </div>

    <!-- Location Permission Prompt -->
    <div
      v-if="locationStatus === 'prompt'"
      class="flex flex-col items-center justify-center px-8 py-20 text-center"
    >
      <div
        class="w-20 h-20 rounded-full flex items-center justify-center mb-6"
        style="background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.08)"
      >
        <Navigation size="36" color="rgba(255,255,255,0.4)" />
      </div>
      <h3 class="text-white text-xl font-semibold font-manrope mb-2">Find Stations Near You</h3>
      <p class="text-gray-400 text-sm font-manrope leading-relaxed mb-8 text-center">
        Allow location access to see the closest MBTA stations to your current position.
      </p>
      <button
        @click="requestLocation"
        class="flex items-center gap-2 bg-orange-500 text-gray-900 border-none rounded-xl px-6 py-3.5 text-sm font-bold cursor-pointer font-manrope"
      >
        <Navigation size="18" />
        Allow Location Access
      </button>
    </div>

    <!-- Loading State -->
    <div v-else-if="locationStatus === 'loading'" class="flex justify-center py-20">
      <div
        class="animate-spin rounded-full h-10 w-10 border-2 border-orange-500 border-t-transparent"
      ></div>
      <p class="text-gray-400 text-sm ml-3">Finding your location...</p>
    </div>

    <!-- Error State -->
    <div
      v-else-if="locationStatus === 'error'"
      class="flex flex-col items-center justify-center px-8 py-20 text-center"
    >
      <div
        class="w-20 h-20 rounded-full flex items-center justify-center mb-6"
        style="background: rgba(230, 57, 70, 0.1); border: 1px solid rgba(230, 57, 70, 0.3)"
      >
        <AlertCircle size="36" color="#E63946" />
      </div>
      <h3 class="text-white text-xl font-semibold font-manrope mb-2">Location Access Denied</h3>
      <p class="text-gray-400 text-sm font-manrope leading-relaxed mb-8 text-center">
        Please enable location access in your browser settings to see nearby stations.
      </p>
      <button
        @click="requestLocation"
        class="flex items-center gap-2 bg-orange-500 text-gray-900 border-none rounded-xl px-6 py-3.5 text-sm font-bold cursor-pointer font-manrope"
      >
        <RefreshCw size="18" />
        Try Again
      </button>
    </div>

    <!-- Stations List -->
    <div v-else-if="stations.length > 0" class="p-4">
      <!-- Current Location Card -->
      <div class="bg-gray-800 rounded-xl p-4 mb-4">
        <div class="flex items-center gap-3">
          <div
            class="w-10 h-10 rounded-full flex items-center justify-center"
            style="background: rgba(255, 140, 0, 0.15)"
          >
            <Navigation size="20" color="#FF8C00" />
          </div>
          <div>
            <p class="text-gray-400 text-xs font-manrope">Your Location</p>
            <p class="text-white text-sm font-manrope">
              {{ currentAddress || `${currentLat.toFixed(4)}, ${currentLon.toFixed(4)}` }}
            </p>
          </div>
        </div>
      </div>

      <!-- Results Count -->
      <div class="flex justify-between items-center mb-3 px-1">
        <h2 class="text-white text-sm font-semibold font-manrope">
          {{ stations.length }} Stations Nearby
        </h2>
        <button @click="refresh" class="text-orange-400 text-xs hover:text-orange-300 transition">
          <RefreshCw size="14" class="inline mr-1" />
          Refresh
        </button>
      </div>

      <!-- Stations List -->
      <div class="space-y-3">
        <div
          v-for="station in stations"
          :key="station.stopId"
          @click="goToStation(station.stopId)"
          class="bg-gray-800 rounded-xl p-4 cursor-pointer transition-all hover:bg-gray-700"
        >
          <div class="flex justify-between items-start">
            <div class="flex-1">
              <div class="flex items-center gap-2 mb-1">
                <span class="text-white font-semibold text-base">{{ station.stopName }}</span>
                <span
                  v-if="station.platformCode"
                  class="text-gray-500 text-xs bg-gray-700 px-2 py-0.5 rounded"
                  >Plat {{ station.platformCode }}</span
                >
              </div>
              <div class="flex items-center gap-3 mt-2">
                <div class="flex items-center gap-1">
                  <Navigation size="12" class="text-orange-400" />
                  <span class="text-gray-400 text-xs">
                    {{ formatDistance(station.distanceMeters) }}
                  </span>
                </div>
                <div class="text-gray-600 text-xs">•</div>
                <div class="text-gray-500 text-xs">ID: {{ station.stopId }}</div>
              </div>
              <div class="mt-2 flex flex-wrap gap-1">
                <LineChip
                  v-for="line in getStationLines(station.stopId)"
                  :key="line"
                  :lineId="line"
                  size="xs"
                />
              </div>
            </div>
            <ChevronRight size="20" class="text-gray-500 flex-shrink-0" />
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div
      v-else-if="locationStatus === 'loaded' && stations.length === 0"
      class="flex flex-col items-center justify-center px-8 py-20 text-center"
    >
      <div
        class="w-20 h-20 rounded-full flex items-center justify-center mb-6"
        style="background: rgba(107, 138, 150, 0.2)"
      >
        <MapPin size="36" color="#6B8A96" />
      </div>
      <h3 class="text-white text-lg font-semibold font-manrope mb-2">No Stations Found</h3>
      <p class="text-gray-400 text-sm font-manrope">
        No MBTA stations within {{ radius }} meters of your location.
      </p>
      <button @click="increaseRadius" class="mt-4 text-orange-400 text-sm hover:underline">
        Increase search radius →
      </button>
    </div>

    <div class="h-6"></div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import {
  ChevronLeft,
  Navigation,
  AlertCircle,
  RefreshCw,
  ChevronRight,
  MapPin,
} from "lucide-vue-next";
import { useToast } from "vue-toastification";
import stationsService from "@/services/api/stations.service";
import LineChip from "@/components/LineChip.vue";

const router = useRouter();
const toast = useToast();

// State
const locationStatus = ref("prompt"); // prompt, loading, loaded, error
const stations = ref([]);
const currentLat = ref(0);
const currentLon = ref(0);
const currentAddress = ref("");
const radius = ref(1000);

// Methods
const requestLocation = () => {
  if (!navigator.geolocation) {
    toast.error("Geolocation is not supported by your browser");
    locationStatus.value = "error";
    return;
  }

  locationStatus.value = "loading";

  navigator.geolocation.getCurrentPosition(
    async (position) => {
      // currentLat.value = position.coords.latitude;
      // currentLon.value = position.coords.longitude;
        currentLat.value =42.35757;
        currentLon.value = -71.05644;//central downtown boston coordinates for testing

      // Get address from coordinates (reverse geocoding)
      await getAddressFromCoords(currentLat.value, currentLon.value);

      await loadNearbyStations();
      locationStatus.value = "loaded";
    },
    (error) => {
      console.error("Location error:", error);
      locationStatus.value = "error";
      toast.error("Unable to get your location. Please check permissions.");
    },
    {
      enableHighAccuracy: true,
      timeout: 10000,
      maximumAge: 0,
    },
  );
};

const getAddressFromCoords = async (lat, lon) => {
  try {
    // Using OpenStreetMap Nominatim for reverse geocoding (free, no API key)
    const response = await fetch(
      `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lon}&zoom=18&addressdetails=1`,
    );
    const data = await response.json();

    if (data.address) {
      const parts = [];
      if (data.address.road) parts.push(data.address.road);
      if (data.address.city || data.address.town)
        parts.push(data.address.city || data.address.town);
      if (parts.length > 0) {
        currentAddress.value = parts.join(", ");
      } else {
        currentAddress.value = `${lat.toFixed(4)}, ${lon.toFixed(4)}`;
      }
    } else {
      currentAddress.value = `${lat.toFixed(4)}, ${lon.toFixed(4)}`;
    }
  } catch (error) {
    console.error("Reverse geocoding failed:", error);
    currentAddress.value = `${lat.toFixed(4)}, ${lon.toFixed(4)}`;
  }
};

const loadNearbyStations = async () => {
  try {
    const response = await stationsService.getNearby(
      currentLat.value,
      currentLon.value,
      radius.value,
      20,
    );

    if (response.success) {
      stations.value = response.data || [];
    } else {
      toast.error(response.message || "Failed to load nearby stations");
    }
  } catch (error) {
    console.error("Failed to load nearby stations:", error);
    toast.error("Unable to load nearby stations");
  }
};

const refresh = () => {
  loadNearbyStations();
  toast.success("Refreshed nearby stations");
};

const increaseRadius = () => {
  if (radius.value === 1000) {
    radius.value = 2000;
  } else if (radius.value === 2000) {
    radius.value = 5000;
  } else {
    radius.value = 1000;
  }
  loadNearbyStations();
};

const formatDistance = (meters) => {
  if (meters < 1000) {
    return `${Math.round(meters)}m away`;
  } else {
    return `${(meters / 1000).toFixed(1)}km away`;
  }
};

const getStationLines = (stopId) => {
  // You can implement this to fetch line info from your data
  // For now, return empty array
  return [];
};

const goToStation = (stopId) => {
  router.push(`/station/${stopId}`);
};

const goBack = () => {
  router.back();
};
</script>
