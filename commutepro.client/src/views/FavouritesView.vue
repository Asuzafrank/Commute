<!-- frontend/src/views/FavouritesView.vue -->
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
      <div class="flex items-center justify-between">
        <div class="flex items-center gap-3">
          <button
            @click="goBack"
            class="w-9 h-9 rounded-lg flex items-center justify-center cursor-pointer transition-all hover:bg-white/5"
            style="background: rgba(255, 255, 255, 0.05); border: none"
          >
            <ChevronLeft size="20" color="rgba(255,255,255,0.75)" />
          </button>
          <h1 class="text-white text-xl font-bold font-manrope">My Stations</h1>
        </div>
        <button
          v-if="favourites.length > 0"
          @click="editMode ? saveOrder() : enterEditMode()"
          class="px-3 py-1.5 rounded-lg text-xs font-semibold font-manrope transition-all"
          :style="
            editMode
              ? { background: '#FF8C00', color: '#243540', border: 'none' }
              : { background: 'none', border: '1px solid rgba(255,255,255,0.1)', color: '#94A3B8' }
          "
        >
          {{ editMode ? "Done" : "Edit" }}
        </button>
      </div>
    </div>

    <!-- Empty State -->
    <div
      v-if="favourites.length === 0 && !loading"
      class="flex flex-col items-center justify-center px-8 py-20 text-center"
    >
      <div
        class="w-20 h-20 rounded-full flex items-center justify-center mb-6"
        style="background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.08)"
      >
        <Star size="36" color="rgba(255,255,255,0.3)" />
      </div>
      <h3 class="text-white text-xl font-semibold font-manrope mb-2">No saved stations</h3>
      <p class="text-gray-400 text-sm font-manrope leading-relaxed mb-8">
        Search for a station to get started. Your saved stations will appear here for quick access.
      </p>
      <button
        @click="goToSearch"
        class="flex items-center gap-2 bg-orange-500 text-gray-900 border-none rounded-xl px-6 py-3.5 text-sm font-bold cursor-pointer font-manrope"
      >
        <Plus size="18" />
        Search for a station
      </button>
    </div>

    <!-- Favourites List -->
    <div v-else class="px-4 py-4">
      <p v-if="editMode" class="text-gray-500 text-xs font-manrope mb-3">
        Drag to reorder your stations
      </p>

      <TransitionGroup name="list" tag="div" class="space-y-2.5">
        <div
          v-for="(station, index) in displayFavourites"
          :key="station.id"
          :draggable="editMode"
          @dragstart="editMode ? onDragStart(index) : null"
          @dragover="editMode ? onDragOver($event, index) : null"
          @dragend="editMode ? onDragEnd() : null"
          class="rounded-xl transition-all cursor-grab active:cursor-grabbing"
          :class="editMode ? 'draggable' : ''"
          :style="{
            background: '#354F5C',
            border: '1px solid #3D5A68',
            boxShadow: '0 4px 20px rgba(0,0,0,0.3)',
          }"
        >
          <div class="flex items-center gap-3 p-4">
            <!-- Drag Handle -->
            <div v-if="editMode" class="flex-shrink-0">
              <GripVertical size="18" color="#6B8A96" />
            </div>

            <!-- Line Color Bar -->
            <div
              class="w-1 h-12 rounded-full flex-shrink-0"
              :style="{ background: getPrimaryLineColor(station) }"
            ></div>

            <!-- Station Info -->
            <div class="flex-1 min-w-0">
              <p class="text-white text-sm font-semibold font-manrope truncate">
                {{ station.stopName }}
              </p>
              <div class="flex items-center gap-1 mt-1 flex-wrap">
                <LineChip
                  v-for="line in getStationLines(station).slice(0, 3)"
                  :key="line"
                  :lineId="line"
                  size="xs"
                />
                <span
                  v-if="getStationLines(station).length > 3"
                  class="text-gray-500 text-[9px] font-manrope"
                >
                  +{{ getStationLines(station).length - 3 }}
                </span>
              </div>
            </div>

            <!-- Next Train Info -->
            <div v-if="!editMode" class="text-right">
              <CountdownTimer
                v-if="getNextArrival(station)"
                :minutes="getCountdown(station)"
                size="md"
              />
              <div v-else class="text-gray-500 text-xs font-manrope">—</div>
            </div>

            <!-- Remove Button (Edit Mode) -->
            <button
              v-if="editMode"
              @click="removeFavourite(station.id)"
              class="w-8 h-8 rounded-full flex items-center justify-center cursor-pointer flex-shrink-0"
              style="background: rgba(230, 57, 70, 0.12)"
            >
              <X size="14" color="#E63946" />
            </button>

            <!-- Chevron (Normal Mode) -->
            <ChevronRight v-else size="16" color="rgba(255,255,255,0.2)" class="flex-shrink-0" />
          </div>
        </div>
      </TransitionGroup>

      <!-- Add Station Button -->
      <button
        @click="goToSearch"
        class="w-full flex items-center gap-3 mt-4 p-4 rounded-xl transition-all cursor-pointer"
        style="background: transparent; border: 1px dashed rgba(255, 255, 255, 0.1)"
        @mouseenter="(e) => (e.currentTarget.style.borderColor = 'rgba(255,255,255,0.2)')"
        @mouseleave="(e) => (e.currentTarget.style.borderColor = 'rgba(255,255,255,0.1)')"
      >
        <div
          class="w-9 h-9 rounded-full flex items-center justify-center flex-shrink-0"
          style="background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.08)"
        >
          <Plus size="18" color="rgba(255,255,255,0.4)" />
        </div>
        <span class="text-gray-500 text-sm font-manrope">Add a station</span>
      </button>
    </div>

    <div class="h-6"></div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import { useToast } from "vue-toastification";
import { ChevronLeft, Star, Plus, GripVertical, X, ChevronRight } from "lucide-vue-next";
import { useAuthStore } from "@/stores/auth.store";
import favouritesService from "@/services/api/favourites.service";
import arrivalsService from "@/services/api/arrivals.service";
import stationsService from "@/services/api/stations.service";
import LineChip from "@/components/LineChip.vue";
import CountdownTimer from "@/components/CountdownTimer.vue";

const router = useRouter();
const toast = useToast();
const authStore = useAuthStore();

// State
const favourites = ref([]);
const stationsData = ref({});
const arrivalsData = ref({});
const loading = ref(true);
const editMode = ref(false);
const dragIndex = ref(null);

// Computed
const displayFavourites = computed(() => {
  return favourites.value;
});

// Methods
const getStationLines = (station) => {
  // This would come from your station data
  // For now, return empty array or derive from route data
  return [];
};

const getPrimaryLineColor = (station) => {
  // Return color based on primary line
  return "#888888";
};

const getNextArrival = (station) => {
  const arrivals = arrivalsData.value[station.stopId];
  if (arrivals && arrivals.length > 0) {
    return arrivals[0];
  }
  return null;
};

const getCountdown = (station) => {
  const arrival = getNextArrival(station);
  if (!arrival) return null;
  const now = Math.floor(Date.now() / 1000);
  const remainingSeconds = arrival.arrivalTime - now;
  if (remainingSeconds <= 0) return 0;
  return Math.floor(remainingSeconds / 60);
};

// Load favourites
const loadFavourites = async () => {
  if (!authStore.isAuthenticated) return;

  loading.value = true;
  try {
    const response = await favouritesService.getFavourites();
    if (response.success) {
      favourites.value = response.data || [];

      // Load station details for each favourite
      for (const fav of favourites.value) {
        await loadStationDetails(fav.stopId);
        await loadStationArrivals(fav.stopId);
      }
    }
  } catch (error) {
    console.error("Failed to load favourites:", error);
    toast.error("Failed to load favourites");
  } finally {
    loading.value = false;
  }
};

// Load station details
const loadStationDetails = async (stopId) => {
  try {
    const response = await stationsService.getStationById(stopId);
    if (response.success) {
      stationsData.value[stopId] = response.data;
    }
  } catch (error) {
    console.error(`Failed to load station ${stopId}:`, error);
  }
};

// Load station arrivals
const loadStationArrivals = async (stopId) => {
  try {
    const data = await arrivalsService.getArrivals(stopId);
    arrivalsData.value[stopId] = data.arrivals || [];
  } catch (error) {
    console.error(`Failed to load arrivals for ${stopId}:`, error);
  }
};

// Remove favourite
const removeFavourite = async (favouriteId) => {
  try {
    const response = await favouritesService.removeFavourite(favouriteId);
    if (response.success) {
      favourites.value = favourites.value.filter((f) => f.id !== favouriteId);
      toast.success("Station removed from favourites");
    }
  } catch (error) {
    console.error("Failed to remove favourite:", error);
    toast.error("Failed to remove station");
  }
};

// Drag and drop reordering
const onDragStart = (index) => {
  dragIndex.value = index;
};

const onDragOver = (event, index) => {
  event.preventDefault();
  if (dragIndex.value === null || dragIndex.value === index) return;

  const newFavourites = [...favourites.value];
  const [draggedItem] = newFavourites.splice(dragIndex.value, 1);
  newFavourites.splice(index, 0, draggedItem);
  favourites.value = newFavourites;
  dragIndex.value = index;
};

const onDragEnd = () => {
  dragIndex.value = null;
};

// Save reordered favourites
const saveOrder = async () => {
  const orderMap = {};
  favourites.value.forEach((fav, idx) => {
    orderMap[fav.id] = idx + 1;
  });

  try {
    await favouritesService.reorderFavourites(orderMap);
    toast.success("Favourites reordered");
    editMode.value = false;
  } catch (error) {
    console.error("Failed to reorder favourites:", error);
    toast.error("Failed to save order");
    await loadFavourites(); // Reload to revert
  }
};

const enterEditMode = () => {
  editMode.value = true;
};

// Navigation
const goBack = () => {
  router.back();
};

const goToSearch = () => {
  router.push("/search");
};

const goToStation = (stopId) => {
  router.push(`/station/${stopId}`);
};

// Countdown timer
let countdownInterval = null;

const startCountdownTimer = () => {
  countdownInterval = setInterval(() => {
    arrivalsData.value = { ...arrivalsData.value };
  }, 1000);
};

onMounted(() => {
  loadFavourites();
  startCountdownTimer();
});

// Cleanup
onUnmounted(() => {
  if (countdownInterval) clearInterval(countdownInterval);
});
</script>

<style scoped>
.list-enter-active,
.list-leave-active {
  transition: all 0.2s ease;
}

.list-enter-from {
  opacity: 0;
  transform: translateY(10px);
}

.list-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

.draggable {
  cursor: grab;
}

.draggable:active {
  cursor: grabbing;
}
</style>
