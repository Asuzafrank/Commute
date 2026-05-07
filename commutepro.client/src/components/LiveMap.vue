<!-- frontend/src/components/LiveMap.vue -->
<template>
  <div class="relative w-full h-full">
    <div ref="mapContainer" class="w-full h-full" style="min-height: 400px"></div>

    <!-- Loading state -->
    <div v-if="loading" class="absolute inset-0 flex items-center justify-center bg-black/50">
      <div
        class="animate-spin rounded-full h-8 w-8 border-2 border-orange-500 border-t-transparent"
      ></div>
    </div>

    <!-- Legend -->
    <div class="absolute bottom-4 right-4 bg-gray-800/90 rounded-lg p-2 text-xs text-white">
      <div class="flex items-center gap-2">
        <div class="w-3 h-3 rounded-full bg-red-500"></div>
        <span>Train</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from "vue";
import L from "leaflet";
import "leaflet/dist/leaflet.css";

// Fix leaflet icon issue
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon-2x.png",
  iconUrl: "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon.png",
  shadowUrl: "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png",
});

const props = defineProps({
  vehicles: {
    type: Array,
    default: () => [],
  },
  center: {
    type: Object,
    default: () => ({ lat: 42.3601, lng: -71.0589 }), // Boston center
  },
  zoom: {
    type: Number,
    default: 12,
  },
});

const mapContainer = ref(null);
let map = null;
let markers = new Map();
const loading = ref(true);

// Create train icon
const getTrainIcon = (bearing) => {
  return L.divIcon({
    html: `<div class="train-marker" style="transform: rotate(${bearing || 0}deg)">
              <div class="w-4 h-4 bg-orange-500 rounded-full border-2 border-white shadow-lg"></div>
              <div class="w-0 h-0 border-l-4 border-r-4 border-b-8 border-transparent border-b-orange-500 absolute -top-2 left-1/2 -translate-x-1/2"></div>
            </div>`,
    className: "custom-train-icon",
    iconSize: [20, 20],
    iconAnchor: [10, 10],
  });
};

// Initialize map
const initMap = () => {
  if (!mapContainer.value) return;

  map = L.map(mapContainer.value).setView([props.center.lat, props.center.lng], props.zoom);

  // Add tile layer (dark theme to match your app)
  L.tileLayer("https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png", {
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OSM</a> &copy; CartoDB',
    subdomains: "abcd",
    maxZoom: 19,
  }).addTo(map);

  loading.value = false;
};

// Update vehicle markers
const updateMarkers = () => {
  if (!map) return;

  // Track which vehicle IDs are still active
  const activeIds = new Set();

  props.vehicles.forEach((vehicle) => {
    activeIds.add(vehicle.id);

    if (vehicle.latitude && vehicle.longitude) {
      const position = [vehicle.latitude, vehicle.longitude];

      if (markers.has(vehicle.id)) {
        // Update existing marker position
        markers.get(vehicle.id).setLatLng(position);
      } else {
        // Create new marker
        const marker = L.marker(position, {
          icon: getTrainIcon(vehicle.bearing),
          title: `${vehicle.routeId} - ${vehicle.tripId}`,
        }).addTo(map);

        // Add popup on click
        marker.bindPopup(`
          <div class="text-sm">
            <strong>${vehicle.routeId}</strong><br>
            Status: ${vehicle.currentStatus}<br>
            Last updated: ${new Date(vehicle.timestamp * 1000).toLocaleTimeString()}
          </div>
        `);

        markers.set(vehicle.id, marker);
      }
    }
  });

  // Remove markers for vehicles no longer in feed
  markers.forEach((marker, id) => {
    if (!activeIds.has(id)) {
      map.removeLayer(marker);
      markers.delete(id);
    }
  });
};

// Watch for vehicle updates
watch(
  () => props.vehicles,
  () => {
    updateMarkers();
  },
  { deep: true },
);

onMounted(() => {
  initMap();
});

onUnmounted(() => {
  if (map) {
    map.remove();
    map = null;
  }
});
</script>

<style scoped>
:deep(.custom-train-icon) {
  background: transparent;
  border: none;
}

.train-marker {
  position: relative;
  width: 20px;
  height: 20px;
}
</style>
