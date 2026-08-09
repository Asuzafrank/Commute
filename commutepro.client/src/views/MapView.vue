<!-- frontend/src/views/MapView.vue -->
<template>
  <div class="fixed inset-0 z-0">
    <!-- Map Container -->
    <div ref="mapContainer" class="w-full h-full"></div>

    <!-- Debug Info -->
    <div
      class="absolute top-4 left-4 z-10 bg-black/80 text-white text-xs px-3 py-2 rounded-lg font-mono"
    >
      🚆 Trains: {{ vehicles.length }}
    </div>

    <!-- Floating Controls -->
    <div class="absolute bottom-24 right-4 z-10 flex flex-col gap-2">
      <button
        @click="zoomIn"
        class="w-12 h-12 rounded-xl bg-gray-900/90 backdrop-blur-md border border-white/10 flex items-center justify-center shadow-lg hover:bg-gray-800 transition"
      >
        <Plus size="20" color="white" />
      </button>
      <button
        @click="zoomOut"
        class="w-12 h-12 rounded-xl bg-gray-900/90 backdrop-blur-md border border-white/10 flex items-center justify-center shadow-lg hover:bg-gray-800 transition"
      >
        <Minus size="20" color="white" />
      </button>
      <button
        @click="zoomToMyLocation"
        class="w-12 h-12 rounded-xl bg-gray-900/90 backdrop-blur-md border border-white/10 flex items-center justify-center shadow-lg hover:bg-gray-800 transition"
      >
        <Crosshair size="20" color="white" />
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="absolute inset-0 bg-black/50 flex items-center justify-center z-20">
      <div
        class="animate-spin rounded-full h-12 w-12 border-3 border-orange-500 border-t-transparent"
      ></div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from "vue";
import { Plus, Minus, Crosshair } from "lucide-vue-next";
import L from "leaflet";
import "leaflet/dist/leaflet.css";

// Fix leaflet icon issue
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon-2x.png",
  iconUrl: "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon.png",
  shadowUrl: "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png",
});

const mapContainer = ref(null);
let map = null;
let vehicleMarkers = [];
let refreshInterval = null;
const loading = ref(true);
const vehicles = ref([]);

// Route colors
const getRouteColor = (routeId) => {
  const colors = {
    Red: "#DA291C",
    Orange: "#ED8B00",
    Blue: "#003DA5",
    Green: "#00843D",
    Mattapan: "#DA291C",
  };
  return colors[routeId] || "#FF8C00";
};

// Create train icon
const createTrainIcon = (color, bearing) => {
  return L.divIcon({
    html: `<div style="transform: rotate(${bearing || 0}deg); display: flex; flex-direction: column; align-items: center;">
            <div style="width: 12px; height: 12px; background: ${color}; border-radius: 50%; border: 2px solid white; box-shadow: 0 0 8px rgba(0,0,0,0.5);"></div>
            <div style="width: 0; height: 0; border-left: 4px solid transparent; border-right: 4px solid transparent; border-top: 6px solid ${color}; margin-top: 2px;"></div>
           </div>`,
    className: "train-marker",
    iconSize: [20, 20],
    iconAnchor: [10, 10],
    popupAnchor: [0, -10],
  });
};

// Initialize map
const initMap = () => {
  if (!mapContainer.value) return;

  map = L.map(mapContainer.value).setView([42.3601, -71.0589], 12);

  // Dark map tiles
  L.tileLayer("https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png", {
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OSM</a> &copy; CartoDB',
    subdomains: "abcd",
    maxZoom: 19,
  }).addTo(map);

  loading.value = false;
};

// Fetch vehicles from API
const fetchVehicles = async () => {
  try {
    const response = await fetch("https://commute-wlkb.onrender.com/api/vehicles");
    const data = await response.json();

    console.log("Vehicles API response:", data);

    if (data.success && data.data) {
      vehicles.value = data.data;
      updateMarkers();
    }
  } catch (error) {
    console.error("Failed to fetch vehicles:", error);
  }
};

// Update markers on map
const updateMarkers = () => {
  if (!map) return;

  // Clear existing markers
  vehicleMarkers.forEach((marker) => {
    map.removeLayer(marker);
  });
  vehicleMarkers = [];

  // Add new markers
  vehicles.value.forEach((vehicle) => {
    if (vehicle.latitude && vehicle.longitude) {
      const color = getRouteColor(vehicle.routeId);
      const marker = L.marker([vehicle.latitude, vehicle.longitude], {
        icon: createTrainIcon(color, vehicle.bearing || 0),
      }).addTo(map);

      // Add popup
      marker.bindPopup(`
        <div style="font-family: sans-serif; min-width: 150px;">
          <strong style="color: ${color}">${vehicle.routeId}</strong><br>
          Status: ${vehicle.currentStatus}<br>
          Last update: ${new Date(vehicle.timestamp * 1000).toLocaleTimeString()}
        </div>
      `);

      vehicleMarkers.push(marker);
    }
  });

  console.log(`Updated ${vehicleMarkers.length} train markers`);
};

// Map controls
const zoomIn = () => map.zoomIn();
const zoomOut = () => map.zoomOut();

const zoomToMyLocation = () => {
  if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(
      (position) => {
        map.setView([position.coords.latitude, position.coords.longitude], 14);
      },
      () => {
        map.setView([42.3601, -71.0589], 12);
      },
    );
  } else {
    map.setView([42.3601, -71.0589], 12);
  }
};

onMounted(() => {
  initMap();
  // Wait a bit for map to initialize then fetch vehicles
  setTimeout(() => {
    fetchVehicles();
    refreshInterval = setInterval(fetchVehicles, 15000);
  }, 1000);
});

onUnmounted(() => {
  if (refreshInterval) clearInterval(refreshInterval);
  if (map) map.remove();
});
</script>

<style scoped>
.train-marker {
  background: transparent;
  border: none;
}

:deep(.leaflet-popup-content-wrapper) {
  background: #1e293b;
  color: white;
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

:deep(.leaflet-popup-tip) {
  background: #1e293b;
}

:deep(.leaflet-popup-close-button) {
  color: white !important;
}
</style>
