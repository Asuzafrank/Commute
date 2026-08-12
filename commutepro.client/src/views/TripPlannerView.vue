<!-- frontend/src/views/TripPlannerView.vue -->
<template>
  <div class="min-h-screen" style="background: #243540">
    <!-- Header -->
    <div class="sticky top-0 z-40 px-5 pt-5 pb-4"
         style="
        background: rgba(28, 42, 52, 0.97);
        backdrop-filter: blur(20px);
        border-bottom: 1px solid rgba(255, 255, 255, 0.07);
      ">
      <div class="flex items-center gap-3">
        <button @click="goBack"
                class="w-9 h-9 rounded-lg flex items-center justify-center cursor-pointer transition-all hover:bg-white/5"
                style="background: rgba(255, 255, 255, 0.05); border: none">
          <ChevronLeft size="20" color="rgba(255,255,255,0.75)" />
        </button>
        <h1 class="text-white text-xl font-bold font-manrope">Plan a Trip</h1>
      </div>
    </div>

    <!-- Trip Form -->
    <div class="p-4 space-y-4">
      <!-- From Station -->
      <div>
        <label class="text-gray-400 text-xs font-semibold font-manrope mb-1 block">From</label>
        <div class="relative">
          <input v-model="fromQuery"
                 type="text"
                 placeholder="Station name or ID..."
                 class="w-full px-4 py-3 rounded-xl text-white text-sm font-manrope outline-none transition-all"
                 style="background: #354f5c; border: 1px solid #3d5a68; caret-color: #ffffff"
                 @input="onFromInput" />
          <div v-if="fromSuggestions.length > 0"
               class="absolute z-10 w-full mt-1 rounded-xl overflow-hidden"
               style="background: #354f5c; border: 1px solid #3d5a68">
            <div v-for="station in fromSuggestions"
                 :key="station.stopId"
                 @click="selectFromStation(station)"
                 class="px-4 py-2 hover:bg-white/10 cursor-pointer transition">
              <div class="text-white text-sm">{{ station.stopName }}</div>
              <div class="text-gray-500 text-xs">ID: {{ station.stopId }}</div>
            </div>
          </div>
        </div>
      </div>

      <!-- To Station -->
      <div>
        <label class="text-gray-400 text-xs font-semibold font-manrope mb-1 block">To</label>
        <div class="relative">
          <input v-model="toQuery"
                 type="text"
                 placeholder="Station name or ID..."
                 class="w-full px-4 py-3 rounded-xl text-white text-sm font-manrope outline-none transition-all"
                 style="background: #354f5c; border: 1px solid #3d5a68; caret-color: #ffffff"
                 @input="onToInput" />
          <div v-if="toSuggestions.length > 0"
               class="absolute z-10 w-full mt-1 rounded-xl overflow-hidden"
               style="background: #354f5c; border: 1px solid #3d5a68">
            <div v-for="station in toSuggestions"
                 :key="station.stopId"
                 @click="selectToStation(station)"
                 class="px-4 py-2 hover:bg-white/10 cursor-pointer transition">
              <div class="text-white text-sm">{{ station.stopName }}</div>
              <div class="text-gray-500 text-xs">ID: {{ station.stopId }}</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Swap Button -->
      <div class="flex justify-center">
        <button @click="swapStations"
                class="w-10 h-10 rounded-full flex items-center justify-center"
                style="background: #3a5060; border: 1px solid #3d5a68">
          <ArrowUpDown size="18" color="#94A3B8" />
        </button>
      </div>

      <!-- Departure Time -->
      <div>
        <label class="text-gray-400 text-xs font-semibold font-manrope mb-1 block">Departure Time</label>
        <input v-model="departureTime"
               type="datetime-local"
               class="w-full px-4 py-3 rounded-xl text-white text-sm font-manrope outline-none transition-all"
               style="background: #354f5c; border: 1px solid #3d5a68; caret-color: #ffffff" />
      </div>

      <!-- Find Trip Button -->
      <button @click="planTrip"
              :disabled="!selectedFrom || !selectedTo || loading"
              class="w-full py-3 rounded-xl text-sm font-bold cursor-pointer transition-all mt-4"
              style="background: #ff8c00; color: #243540"
              :class="{ 'opacity-50 cursor-not-allowed': !selectedFrom || !selectedTo || loading }">
        {{ loading ? "Finding..." : "Find Earliest Trip" }}
      </button>

      <!-- Error Message -->
      <div v-if="error"
           class="flex items-center gap-2 p-3 rounded-xl"
           style="background: rgba(230, 57, 70, 0.1); border: 1px solid rgba(230, 57, 70, 0.3)">
        <AlertCircle size="16" color="#E63946" />
        <span class="text-red-400 text-sm">{{ error }}</span>
      </div>
    </div>

    <!-- Trip Result -->
    <div v-if="tripResult && tripResult.success" class="px-4 pb-4">
      <div class="bg-gray-800 rounded-xl p-4">
        <div class="flex justify-between items-center mb-4">
          <div>
            <p class="text-gray-400 text-xs">From</p>
            <p class="text-white font-semibold">{{ tripResult.fromStation }}</p>
          </div>
          <ArrowRight size="20" color="#FF8C00" />
          <div class="text-right">
            <p class="text-gray-400 text-xs">To</p>
            <p class="text-white font-semibold">{{ tripResult.toStation }}</p>
          </div>
        </div>

        <div class="flex justify-between items-center mb-4 p-3 rounded-lg"
             style="background: #354f5c">
          <div>
            <p class="text-gray-400 text-xs">Departure</p>
            <p class="text-white text-lg font-bold">{{ formatTime(tripResult.departureTime) }}</p>
          </div>
          <div class="text-center">
            <Clock size="20" color="#FF8C00" class="mx-auto mb-1" />
            <p class="text-white text-sm">{{ tripResult.totalDurationMinutes }} min</p>
          </div>
          <div class="text-right">
            <p class="text-gray-400 text-xs">Arrival</p>
            <p class="text-white text-lg font-bold">{{ formatTime(tripResult.arrivalTime) }}</p>
          </div>
        </div>

        <!-- Legs -->
        <div class="space-y-3">
          <div v-for="(leg, index) in tripResult.legs"
               :key="index"
               class="p-3 rounded-lg"
               style="background: #2c3e4a">
            <div class="flex items-center gap-2 mb-2">
              <div class="w-4 h-4 rounded-full"
                   :style="{ background: `#${leg.routeColor.trim()}` }"></div>
              <span class="text-white font-semibold text-sm">Route {{ leg.routeName }}</span>
              <span class="text-gray-400 text-xs ml-auto">{{ leg.durationMinutes }} min</span>
            </div>
            <div class="flex justify-between items-center">
              <div>
                <p class="text-gray-400 text-xs">{{ formatTime(leg.departureTime) }}</p>
                <p class="text-white text-sm">{{ leg.fromStopName }}</p>
              </div>
              <ArrowRight size="16" color="#6B8A96" />
              <div class="text-right">
                <p class="text-gray-400 text-xs">{{ formatTime(leg.arrivalTime) }}</p>
                <p class="text-white text-sm">{{ leg.toStopName }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="tripResult && !tripResult.success" class="px-4">
      <div class="bg-yellow-900/30 border border-yellow-600 rounded-xl p-4">
        <p class="text-yellow-500 text-sm">{{ tripResult.message || "No trips found" }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import { ChevronLeft, ArrowUpDown, ArrowRight, Clock, AlertCircle } from "lucide-vue-next";
import { useToast } from "vue-toastification";
import tripsService from "@/services/api/trips.service";
import stationsService from "@/services/api/stations.service";

const router = useRouter();
const toast = useToast();

// Form state
const fromQuery = ref("");
const toQuery = ref("");
const selectedFrom = ref(null);
const selectedTo = ref(null);
const fromSuggestions = ref([]);
const toSuggestions = ref([]);
const departureTime = ref("");
const loading = ref(false);
const error = ref("");
const tripResult = ref(null);

// Methods
const onFromInput = async () => {
  if (fromQuery.value.length < 2) {
    fromSuggestions.value = [];
    return;
  }
  try {
    const response = await stationsService.search(fromQuery.value);
    if (response.success) {
      fromSuggestions.value = response.data.slice(0, 5);
    }
  } catch (err) {
    console.error("Search failed:", err);
  }
};

const onToInput = async () => {
  if (toQuery.value.length < 2) {
    toSuggestions.value = [];
    return;
  }
  try {
    const response = await stationsService.search(toQuery.value);
    if (response.success) {
      toSuggestions.value = response.data.slice(0, 5);
    }
  } catch (err) {
    console.error("Search failed:", err);
  }
};

const selectFromStation = (station) => {
  selectedFrom.value = station;
  fromQuery.value = station.stopName;
  fromSuggestions.value = [];
};

const selectToStation = (station) => {
  selectedTo.value = station;
  toQuery.value = station.stopName;
  toSuggestions.value = [];
};

const swapStations = () => {
  const temp = selectedFrom.value;
  selectedFrom.value = selectedTo.value;
  selectedTo.value = temp;
  fromQuery.value = selectedFrom.value?.stopName || "";
  toQuery.value = selectedTo.value?.stopName || "";
};

const planTrip = async () => {
  if (!selectedFrom.value || !selectedTo.value) {
    error.value = "Please select both stations";
    return;
  }

  loading.value = true;
  error.value = "";
  tripResult.value = null;

  try {
    const departureDateTime = departureTime.value ? new Date(departureTime.value) : new Date();
    const response = await tripsService.planDirectTrip(
      selectedFrom.value.stopId,
      selectedTo.value.stopId,
      departureDateTime.toISOString(),
    );

    if (response.success) {
      tripResult.value = response.data;
    } else {
      error.value = response.message || "No trips found";
    }
  } catch (err) {
    console.error("Trip planning failed:", err);
    error.value = "Failed to plan trip. Please try again.";
  } finally {
    loading.value = false;
  }
};

const formatTime = (dateString) => {
  if (!dateString) return "";
  const date = new Date(dateString);
  return date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
};

const goBack = () => {
  router.back();
};

// Set default departure time to now
onMounted(() => {
  const now = new Date();
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, "0");
  const day = String(now.getDate()).padStart(2, "0");
  const hours = String(now.getHours()).padStart(2, "0");
  const minutes = String(now.getMinutes()).padStart(2, "0");
  departureTime.value = `${year}-${month}-${day}T${hours}:${minutes}`;
});
</script>
