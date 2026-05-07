<!-- frontend/src/views/StationDetailsView.vue -->
<template>
  <div class="min-h-screen" style="background: #243540">
    <!-- Toast Notification -->
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="-translate-y-12 opacity-0"
      enter-to-class="translate-y-0 opacity-100"
      leave-active-class="transition-all duration-200 ease-in"
      leave-from-class="translate-y-0 opacity-100"
      leave-to-class="-translate-y-12 opacity-0"
    >
      <div
        v-if="showToast"
        class="fixed top-4 left-1/2 z-50 flex items-center gap-2 px-4 py-3 rounded-xl"
        style="
          transform: translateX(-50%);
          max-width: 320px;
          background: #3a5060;
          border: 1px solid #3d5a68;
        "
      >
        <CheckCircle size="16" color="#94A3B8" />
        <span class="text-white text-xs font-manrope">
          {{ isFavourite ? "Added to favourites" : "Removed from favourites" }}
        </span>
      </div>
    </Transition>

    <!-- Header -->
    <div
      class="sticky top-0 z-40"
      style="background: #243540; border-bottom: 1px solid rgba(255, 255, 255, 0.07)"
    >
      <div class="flex items-center justify-between px-4 py-3">
        <button
          @click="goBack"
          class="w-10 h-10 rounded-full flex items-center justify-center cursor-pointer"
          style="background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.08)"
        >
          <ChevronLeft size="20" color="rgba(255,255,255,0.75)" />
        </button>

        <h1 class="text-white text-lg font-semibold font-manrope truncate max-w-[200px]">
          {{ stationName }}
        </h1>

        <button
          @click="toggleFavourite"
          class="w-10 h-10 rounded-full flex items-center justify-center cursor-pointer"
          :style="{
            background: isFavourite ? 'rgba(255,140,0,0.15)' : 'rgba(255,255,255,0.05)',
            border: '1px solid rgba(255,255,255,0.08)',
          }"
        >
          <Heart
            size="20"
            :color="isFavourite ? '#FFFFFF' : '#4A6478'"
            :fill="isFavourite ? '#FFFFFF' : 'none'"
          />
        </button>
      </div>
    </div>

    <!-- Live Status Banner -->
    <div
      class="px-4 py-3 flex items-center justify-between"
      style="border-bottom: 1px solid rgba(255, 255, 255, 0.07)"
    >
      <StatusPill
        :status="liveStatus"
        :updatedSecondsAgo="liveStatus === 'live' ? secondsAgo : undefined"
      />
      <div v-if="hasDelays" class="flex items-center gap-1">
        <span class="text-amber-400 text-xs font-manrope"> Delayed service </span>
      </div>
    </div>

    <!-- Line Filter Chips -->
    <div
      class="flex items-center gap-2 px-4 py-3 overflow-x-auto"
      style="scrollbar-width: none; border-bottom: 1px solid rgba(255, 255, 255, 0.07)"
    >
      <button
        @click="activeLineFilter = 'all'"
        class="px-3.5 py-1.5 rounded-full text-xs font-semibold whitespace-nowrap transition-all"
        :class="
          activeLineFilter === 'all'
            ? 'bg-white/10 text-white border-white/30'
            : 'bg-transparent text-gray-500 border-white/10'
        "
        style="border-width: 1px"
      >
        All Lines
      </button>
      <button
        v-for="line in stationLines"
        :key="line"
        @click="activeLineFilter = line"
        class="px-3.5 py-1.5 rounded-full text-xs font-semibold whitespace-nowrap transition-all"
        :style="{
          borderWidth: '1px',
          borderColor: activeLineFilter === line ? getLineColor(line) : 'rgba(255,255,255,0.08)',
          background: activeLineFilter === line ? `${getLineColor(line)}22` : 'transparent',
          color: activeLineFilter === line ? getLineColor(line) : '#5B7888',
        }"
      >
        {{ getLineName(line) }}
      </button>
    </div>

    <!-- No Service State -->
    <div
      v-if="demoState === 'no-service' || (arrivals.length === 0 && !loading)"
      class="flex flex-col items-center justify-center px-8 py-16 text-center"
    >
      <div
        class="w-18 h-18 rounded-full flex items-center justify-center mb-5"
        style="background: rgba(107, 138, 150, 0.2)"
      >
        <Train size="32" color="#6B8A96" />
      </div>
      <h3 class="text-white text-lg font-semibold font-manrope mb-2">No trains running</h3>
      <p class="text-gray-400 text-sm font-manrope leading-relaxed">
        There are no services at this station right now. Please check back later.
      </p>
    </div>

    <!-- Loading State -->
    <div v-else-if="loading" class="flex justify-center py-12">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-orange-500"></div>
    </div>

    <!-- Arrivals List -->
    <div v-else>
      <TransitionGroup name="arrival" tag="div" class="divide-y divide-white/5">
        <button
          v-for="arrival in filteredArrivals"
          :key="arrival.tripId"
          @click="goToTripDetails(arrival.tripId)"
          class="w-full flex items-center gap-3 px-4 py-4 text-left cursor-pointer transition-all hover:bg-white/5"
          :style="{ opacity: arrival.isCancelled ? 0.6 : 1 }"
        >
          <!-- Line pill -->
          <div
            class="w-1 h-12 rounded-full flex-shrink-0"
            :style="{ background: getLineColor(arrival.routeId) }"
          />

          <!-- Destination + Platform -->
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 mb-1 flex-wrap">
              <span class="text-white text-sm font-semibold font-manrope truncate">
                {{ arrival.headsign }}
              </span>
              <span
                class="bg-gray-700 text-gray-400 text-[10px] font-semibold px-1.5 py-0.5 rounded font-manrope flex-shrink-0"
              >
                Plat. {{ arrival.platform || "Any" }}
              </span>
              <span
                v-if="arrival.isCancelled"
                class="bg-red-500/20 text-red-400 text-[10px] font-semibold px-1.5 py-0.5 rounded font-manrope flex-shrink-0"
              >
                Cancelled
              </span>
              <span
                v-else-if="arrival.delay && arrival.delay > 0"
                class="bg-amber-500/20 text-amber-400 text-[10px] font-semibold px-1.5 py-0.5 rounded font-manrope flex-shrink-0"
              >
                Delayed
              </span>
              <!-- ML Prediction Badge -->
              <span
                v-if="arrival.predictedDelayDisplay"
                class="px-1.5 py-0.5 rounded text-[10px] font-semibold font-manrope flex-shrink-0"
                style="background: #9333ea20; color: #9333ea"
                :title="'Machine Learning prediction based on historical patterns'"
              >
                🤖 {{ arrival.predictedDelayDisplay }}
              </span>
            </div>

            <LineChip :lineId="arrival.routeId" size="xs" />

            <!-- Delay display -->
            <div
              v-if="arrival.delay && arrival.delay > 0 && !arrival.isCancelled"
              class="flex items-center gap-2 mt-1"
            >
              <span class="text-gray-500 text-[11px] font-mono line-through">
                {{ arrival.displayTime }}
              </span>
              <span class="text-amber-400 text-[11px] font-mono">
                {{ formatDelayTime(arrival.delay) }}
              </span>
            </div>
            <div v-else-if="!arrival.isCancelled" class="mt-1">
              <span class="text-gray-500 text-[11px] font-mono">
                {{ arrival.displayTime }}
              </span>
            </div>
          </div>

          <!-- Countdown -->
          <div class="flex flex-col items-end gap-1">
            <div
              v-if="arrival.isCancelled"
              class="w-9 h-9 rounded-full flex items-center justify-center"
              style="background: rgba(230, 57, 70, 0.1)"
            >
              <AlertTriangle size="16" color="#E63946" />
            </div>
            <CountdownTimer v-else :minutes="getLiveCountdown(arrival.arrivalTime)" size="md" />
          </div>
        </button>
      </TransitionGroup>

      <!-- Last Updated -->
      <div
        class="flex items-center gap-2 px-4 py-3"
        style="border-top: 1px solid rgba(255, 255, 255, 0.06)"
      >
        <Clock size="12" color="#6B8A96" />
        <span class="text-gray-500 text-[11px] font-manrope">
          Last updated: {{ secondsAgo }} seconds ago
        </span>
        <span v-if="isDataStale" class="text-amber-400 text-[11px] font-manrope ml-auto">
          Data may be stale
        </span>
        <span v-else-if="signalRConnected" class="text-green-500 text-[11px] font-manrope ml-auto">
          ● Live
        </span>
      </div>
    </div>

    <!-- Track this station button -->
    <div class="px-4 py-4" style="border-top: 1px solid rgba(255, 255, 255, 0.07)">
      <button
        @click="toggleFavourite"
        class="w-full flex items-center justify-center gap-2 rounded-xl py-3.5 text-sm font-bold cursor-pointer transition-all"
        :style="{
          background: isFavourite ? 'rgba(255,255,255,0.05)' : '#FF8C00',
          border: `1px solid ${isFavourite ? 'rgba(255,255,255,0.1)' : '#FF8C00'}`,
          color: isFavourite ? '#5B7888' : '#1C2B33',
        }"
      >
        <Heart size="18" :fill="isFavourite ? 'none' : 'none'" />
        {{ isFavourite ? "Saved to Favourites" : "Track this station" }}
      </button>
    </div>

    <!-- Demo State Switcher (Development only) -->
    <div
      v-if="isDevelopment"
      class="flex items-center justify-center gap-2 px-4 py-3 border-t border-white/5"
    >
      <span class="text-[10px] text-gray-500 font-manrope">Demo:</span>
      <button
        v-for="state in demoStates"
        :key="state.value"
        @click="demoState = state.value"
        class="px-2 py-1 rounded-full text-[10px] font-semibold transition-all"
        :class="
          demoState === state.value ? 'bg-white/10 text-white' : 'bg-transparent text-gray-500'
        "
        style="border: 1px solid rgba(255, 255, 255, 0.1)"
      >
        {{ state.label }}
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useToast } from "vue-toastification";
import { ChevronLeft, Heart, Clock, AlertTriangle, Train, CheckCircle } from "lucide-vue-next";
import { useAuthStore } from "@/stores/auth.store";
import arrivalsService from "@/services/api/arrivals.service";
import favouritesService from "@/services/api/favourites.service";
import { signalRService } from "@/services/signalr.service";
import LineChip from "@/components/LineChip.vue";
import CountdownTimer from "@/components/CountdownTimer.vue";
import StatusPill from "@/components/StatusPill.vue";

const route = useRoute();
const router = useRouter();
const toast = useToast();
const authStore = useAuthStore();

// State
const stationId = ref(route.params.stopId);
const stationName = ref("");
const arrivals = ref([]);
const loading = ref(true);
const isFavourite = ref(false);
const favouriteLoading = ref(false);
const signalRConnected = ref(false);
const isDataStale = ref(false);
const secondsAgo = ref(0);
const activeLineFilter = ref("all");
const demoState = ref("normal");
const showToast = ref(false);

// Demo states
const demoStates = [
  { value: "normal", label: "Normal" },
  { value: "no-service", label: "No Service" },
];

// Computed
const isDevelopment = import.meta.env.DEV;

const stationLines = computed(() => {
  const lines = new Set();
  arrivals.value.forEach((arrival) => {
    if (arrival.routeId) lines.add(arrival.routeId);
  });
  return Array.from(lines);
});

const filteredArrivals = computed(() => {
  if (activeLineFilter.value === "all") return arrivals.value;
  return arrivals.value.filter((a) => a.routeId === activeLineFilter.value);
});

const liveStatus = computed(() => {
  if (demoState.value === "no-service") return "no-service";
  if (hasDelays.value) return "delayed";
  return "live";
});

const hasDelays = computed(() => {
  return arrivals.value.some((a) => a.delay && a.delay > 0 && !a.isCancelled);
});

// Methods
const getLineColor = (routeId) => {
  const colors = {
    Red: "#DA291C",
    Orange: "#ED8B00",
    Blue: "#003DA5",
    Green: "#00843D",
    Mattapan: "#DA291C",
    741: "#7C878E",
    742: "#7C878E",
    743: "#7C878E",
    746: "#7C878E",
  };
  return colors[routeId] || "#888888";
};

const getLineName = (routeId) => {
  if (/^\d+$/.test(routeId)) return routeId;
  return routeId;
};

const getLiveCountdown = (arrivalTime) => {
  const now = Math.floor(Date.now() / 1000);
  const remainingSeconds = arrivalTime - now;
  if (remainingSeconds <= 0) return 0;
  return Math.floor(remainingSeconds / 60);
};

const formatDelayTime = (delaySeconds) => {
  if (!delaySeconds) return "";
  const minutes = Math.floor(delaySeconds / 60);
  return `${minutes} min`;
};

// Load station data
const loadStationData = async () => {
  loading.value = true;
  try {
    const data = await arrivalsService.getArrivals(stationId.value);
    stationName.value = data.stationName;
    arrivals.value = data.arrivals || [];
    isDataStale.value = data.isDataStale;

    if (authStore.isAuthenticated) {
      await checkFavouriteStatus();
    }
  } catch (error) {
    console.error("Failed to load station:", error);
    toast.error("Unable to load station information");
  } finally {
    loading.value = false;
  }
};

// Favourite methods
const checkFavouriteStatus = async () => {
  try {
    const response = await favouritesService.isFavourite(stationId.value);
    if (response.success) {
      isFavourite.value = response.data;
    }
  } catch (error) {
    console.error("Failed to check favourite status:", error);
  }
};

const toggleFavourite = async () => {
  if (!authStore.isAuthenticated) {
    toast.info("Please login to save favourites");
    router.push("/login");
    return;
  }

  favouriteLoading.value = true;
  try {
    if (isFavourite.value) {
      const favourites = await favouritesService.getFavourites();
      const fav = favourites.data?.find((f) => f.stopId === stationId.value);
      if (fav) {
        await favouritesService.removeFavourite(fav.id);
        isFavourite.value = false;
        showToastMessage();
      }
    } else {
      await favouritesService.addFavourite(stationId.value);
      isFavourite.value = true;
      showToastMessage();
    }
  } catch (error) {
    console.error("Failed to toggle favourite:", error);
    toast.error("Unable to update favourites");
  } finally {
    favouriteLoading.value = false;
  }
};

const showToastMessage = () => {
  showToast.value = true;
  setTimeout(() => {
    showToast.value = false;
  }, 2500);
};

// SignalR handlers
const handleArrivalsUpdate = (data) => {
  if (data.stationId === stationId.value) {
    if (data.arrivals && data.arrivals.length > 0) {
      arrivals.value = data.arrivals;
    }
    isDataStale.value = data.isStale || false;
  }
};

const handleSignalRConnected = () => {
  signalRConnected.value = true;
};

const handleSignalRDisconnected = () => {
  signalRConnected.value = false;
};

// Connect SignalR
const connectSignalR = async () => {
  const token = localStorage.getItem("accessToken");
  signalRService.onArrivalsUpdate = handleArrivalsUpdate;
  signalRService.onConnected = handleSignalRConnected;
  signalRService.onDisconnected = handleSignalRDisconnected;
  await signalRService.connect(stationId.value, token);
  signalRConnected.value = signalRService.isConnected;
};

const disconnectSignalR = async () => {
  await signalRService.disconnect();
  signalRConnected.value = false;
};

// Countdown timer
let countdownInterval = null;
let secondsInterval = null;

const startCountdownTimer = () => {
  countdownInterval = setInterval(() => {
    arrivals.value = [...arrivals.value];
  }, 1000);

  secondsInterval = setInterval(() => {
    secondsAgo.value += 1;
  }, 1000);
};

// Navigation
const goBack = () => {
  router.back();
};

const goToTripDetails = (tripId) => {
  router.push(`/trip/${tripId}`);
};

// Watch for station ID changes
watch(
  () => route.params.stopId,
  async (newStationId) => {
    if (newStationId && newStationId !== stationId.value) {
      stationId.value = newStationId;
      await disconnectSignalR();
      await loadStationData();
      await connectSignalR();
      secondsAgo.value = 0;
    }
  },
);

onMounted(async () => {
  await loadStationData();
  await connectSignalR();
  startCountdownTimer();
});

onUnmounted(() => {
  disconnectSignalR();
  if (countdownInterval) clearInterval(countdownInterval);
  if (secondsInterval) clearInterval(secondsInterval);
});
</script>

<style scoped>
.arrival-enter-active,
.arrival-leave-active {
  transition: all 0.2s ease-out;
}

.arrival-enter-from {
  opacity: 0;
  transform: translateY(20px);
}

.arrival-leave-to {
  opacity: 0;
  transform: translateY(-20px);
}
</style>
