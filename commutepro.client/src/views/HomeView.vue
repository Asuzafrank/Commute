<!-- frontend/src/views/HomeView.vue -->
<template>
  <div class="min-h-screen" style="background: #243540">
    <!-- Stale data banner -->
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="-translate-y-full opacity-0"
      enter-to-class="translate-y-0 opacity-100"
      leave-active-class="transition-all duration-200 ease-in"
      leave-from-class="translate-y-0 opacity-100"
      leave-to-class="-translate-y-full opacity-0"
    >
      <div
        v-if="showStaleBanner"
        class="flex items-center justify-between px-4 py-3"
        style="background: #f5a623; z-index: 60"
      >
        <div class="flex items-center gap-2">
          <AlertTriangle size="14" color="#1A1A1A" />
          <span class="text-xs font-semibold text-black font-manrope">
            Live data may be outdated ({{ staleSeconds }}s ago) — tap to refresh
          </span>
        </div>
        <button @click="refreshData" class="bg-transparent border-none cursor-pointer p-1">
          <RefreshCw size="16" color="#1A1A1A" />
        </button>
      </div>
    </Transition>

    <!-- Top bar with Map button -->
    <div class="flex items-center justify-between px-5 pt-4 pb-2">
      <img src="/src/assets/logo.png" alt="CommutePro" class="h-9 w-auto" />
      <div class="flex items-center gap-2">
        <!-- Map Button -->
        <button
          @click="goToMap"
          class="w-9 h-9 rounded-full flex items-center justify-center cursor-pointer flex-shrink-0"
          style="background: rgba(255, 255, 255, 0.08); border: 1px solid rgba(255, 255, 255, 0.1)"
        >
          <Map size="18" color="rgba(255,255,255,0.75)" />
        </button>
        <!-- Profile Button -->
        <button
          @click="goToProfile"
          class="w-9 h-9 rounded-full flex items-center justify-center cursor-pointer flex-shrink-0"
          style="background: rgba(255, 255, 255, 0.08); border: 1px solid rgba(255, 255, 255, 0.1)"
        >
          <User size="18" color="rgba(255,255,255,0.75)" />
        </button>
      </div>
    </div>

    <!-- Greeting -->
    <div class="px-5 pb-5">
      <p class="text-gray-400 text-sm font-manrope">
        {{ greeting }}, <span class="text-white font-semibold">{{ userName || "Commuter" }}</span>
      </p>
    </div>

    <!-- Empty State (No Favourites) -->
    <div
      v-if="isAuthenticated && favouriteStations.length === 0"
      class="flex flex-col items-center justify-center px-8 py-12 text-center"
    >
      <div
        class="w-20 h-20 rounded-full flex items-center justify-center mb-6"
        style="background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.1)"
      >
        <Star size="36" color="rgba(255,255,255,0.4)" />
      </div>
      <h3 class="text-white text-xl font-semibold font-manrope mb-2">No favourite stations yet</h3>
      <p class="text-gray-400 text-sm font-manrope leading-relaxed mb-8">
        Add your first station to get started. Your next train will always be a tap away.
      </p>
      <button
        @click="goToSearch"
        class="flex items-center gap-2 bg-orange-500 text-gray-900 border-none rounded-xl px-6 py-3.5 text-sm font-bold cursor-pointer font-manrope"
      >
        <Plus size="18" />
        Add your first station
      </button>
    </div>

    <!-- Guest User View (Not Logged In) -->
    <div
      v-else-if="!isAuthenticated"
      class="flex flex-col items-center justify-center px-8 py-12 text-center"
    >
      <div
        class="w-20 h-20 rounded-full flex items-center justify-center mb-6"
        style="background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.1)"
      >
        <User size="36" color="rgba(255,255,255,0.4)" />
      </div>
      <h3 class="text-white text-xl font-semibold font-manrope mb-2">Sign in to CommutePro</h3>
      <p class="text-gray-400 text-sm font-manrope leading-relaxed mb-8">
        Save your favourite stations and get personalized alerts.
      </p>
      <button
        @click="goToLogin"
        class="flex items-center gap-2 bg-orange-500 text-gray-900 border-none rounded-xl px-6 py-3.5 text-sm font-bold cursor-pointer font-manrope"
      >
        Sign In
      </button>
    </div>

    <!-- Logged In with Favourites -->
    <div v-else>
      <!-- Favourite stations header -->
      <div class="flex items-center justify-between px-5 mb-3">
        <h2 class="text-white text-base font-semibold font-manrope">Favourite Stations</h2>
        <button
          @click="goToFavourites"
          class="bg-transparent border-none text-gray-500 text-xs font-medium cursor-pointer font-manrope"
        >
          Manage
        </button>
      </div>

      <!-- Station cards horizontal scroll -->
      <div class="flex gap-3 px-5 pb-4 overflow-x-auto" style="scrollbar-width: none">
        <div
          v-for="station in favouriteStations"
          :key="station.stopId"
          @click="goToStation(station.stopId)"
          class="min-w-[210px] cursor-pointer rounded-xl p-4 transition-all hover:-translate-y-0.5"
          style="
            background: #354f5c;
            border: 1px solid #3d5a68;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
          "
        >
          <div class="text-white text-sm font-semibold font-manrope mb-1">
            {{ station.stopName }}
          </div>
          <div
            v-if="getNextArrival(station.stopId)"
            class="text-gray-400 text-xs font-manrope mb-3"
          >
            to {{ getNextArrival(station.stopId)?.headsign || "Unknown" }}
          </div>

          <div class="flex items-end justify-between mb-3">
            <div class="relative">
              <CountdownTimer :minutes="getCountdown(station.stopId)" size="lg" />
              <div
                v-if="
                  getNextArrival(station.stopId)?.delay && getNextArrival(station.stopId).delay > 0
                "
                class="absolute -top-2 -right-2 flex items-center gap-1 rounded-full px-2 py-0.5"
                style="background: #f5a623"
              >
                <span class="text-[10px] font-bold text-black font-manrope whitespace-nowrap">
                  Delayed · +{{ Math.round(getNextArrival(station.stopId).delay / 60) }}m
                </span>
              </div>
            </div>
            <ChevronRight size="16" color="rgba(255,255,255,0.2)" />
          </div>

          <div class="flex flex-wrap gap-1">
            <LineChip
              v-for="line in station.lines?.slice(0, 3)"
              :key="line"
              :lineId="line"
              size="xs"
            />
            <span
              v-if="station.lines?.length > 3"
              class="text-[9px] text-gray-500 font-manrope self-center"
            >
              +{{ station.lines.length - 3 }}
            </span>
          </div>
        </div>

        <!-- Add station card -->
        <button
          @click="goToSearch"
          class="min-w-[100px] flex flex-col items-center justify-center rounded-xl border border-dashed border-white/10 bg-transparent cursor-pointer gap-2 p-4"
        >
          <div
            class="w-9 h-9 rounded-full flex items-center justify-center"
            style="
              background: rgba(255, 255, 255, 0.05);
              border: 1px solid rgba(255, 255, 255, 0.1);
            "
          >
            <Plus size="18" color="rgba(255,255,255,0.5)" />
          </div>
          <span class="text-[11px] text-gray-500 font-manrope text-center">Add station</span>
        </button>
      </div>

      <!-- Search bar -->
      <div class="px-5 mb-6">
        <button
          @click="goToSearch"
          class="w-full flex items-center gap-3 rounded-xl p-3.5 text-left cursor-pointer"
          style="background: rgba(255, 255, 255, 0.04); border: 1px solid rgba(255, 255, 255, 0.08)"
        >
          <Search size="18" color="#4A6478" />
          <span class="text-gray-500 text-sm font-manrope">Search for a station…</span>
        </button>
      </div>

      <!-- Recent alerts -->
      <div class="px-5">
        <div class="flex items-center justify-between mb-3">
          <h2 class="text-white text-base font-semibold font-manrope">Recent Alerts</h2>
          <button
            @click="goToAlerts"
            class="bg-transparent border-none text-gray-500 text-xs font-medium cursor-pointer font-manrope"
          >
            View all
          </button>
        </div>
        <div class="space-y-2">
          <button
            v-for="alert in recentAlerts"
            :key="alert.id"
            @click="goToAlert(alert.id)"
            class="w-full flex items-center gap-3 rounded-lg p-3 text-left cursor-pointer relative overflow-hidden"
            :style="{ background: '#354F5C', borderLeft: `3px solid ${getAlertColor(alert)}` }"
          >
            <component
              :is="getAlertIcon(alert)"
              size="14"
              :color="getAlertColor(alert)"
              class="flex-shrink-0"
            />
            <div class="flex-1 min-w-0">
              <p class="text-white text-xs font-manrope truncate">{{ alert.headerText }}</p>
              <div class="flex items-center gap-2 mt-1">
                <LineChip
                  v-if="alert.affectedRoutes?.[0]"
                  :lineId="alert.affectedRoutes[0]"
                  size="xs"
                />
                <span class="text-gray-500 text-[10px] font-manrope">{{
                  formatTimeAgo(alert.startTime)
                }}</span>
              </div>
            </div>
            <div
              v-if="!alert.isRead"
              class="w-1.5 h-1.5 rounded-full bg-white/50 absolute top-3 right-3"
            ></div>
          </button>
        </div>
      </div>
    </div>

    <div class="h-6"></div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import { useToast } from "vue-toastification";
import {
  Star,
  Search,
  ChevronRight,
  Plus,
  User,
  AlertTriangle,
  RefreshCw,
  Bell,
  Map,
} from "lucide-vue-next";
import { useAuthStore } from "@/stores/auth.store";
import favouritesService from "@/services/api/favourites.service";
import arrivalsService from "@/services/api/arrivals.service";
import alertsService from "@/services/api/alerts.service";
import LineChip from "@/components/LineChip.vue";
import CountdownTimer from "@/components/CountdownTimer.vue";

const router = useRouter();
const toast = useToast();
const authStore = useAuthStore();

// State
const showStaleBanner = ref(false);
const staleSeconds = ref(0);
const favourites = ref([]);
const arrivalsData = ref({});
const alerts = ref([]);
let countdownInterval = null;
let staleInterval = null;

// Computed
const isAuthenticated = computed(() => authStore.isAuthenticated);
const userName = computed(() => authStore.user?.userName || "");

const favouriteStations = computed(() => {
  return favourites.value;
});

const recentAlerts = computed(() => {
  return (alerts.value || []).slice(0, 2);
});

const greeting = computed(() => {
  const hour = new Date().getHours();
  if (hour < 12) return "Good morning";
  if (hour < 17) return "Good afternoon";
  return "Good evening";
});

// Methods
const getNextArrival = (stopId) => {
  const arrivals = arrivalsData.value[stopId];
  if (arrivals && arrivals.length > 0) {
    return arrivals[0];
  }
  return null;
};

const getCountdown = (stopId) => {
  const arrival = getNextArrival(stopId);
  if (!arrival) return null;

  const now = Math.floor(Date.now() / 1000);
  const remainingSeconds = arrival.arrivalTime - now;
  if (remainingSeconds <= 0) return 0;
  return Math.floor(remainingSeconds / 60);
};

const getAlertColor = (alert) => {
  if (alert.severity === "SEVERE") return "#E63946";
  if (alert.severity === "WARNING") return "#F5A623";
  return "#009FE0";
};

const getAlertIcon = (alert) => {
  if (alert.effect === "Cancellation") return AlertTriangle;
  if (alert.effect === "Delay") return AlertTriangle;
  return Bell;
};

const formatTimeAgo = (dateString) => {
  if (!dateString) return "recent";
  const date = new Date(dateString);
  const minutes = Math.floor((Date.now() - date.getTime()) / 60000);
  if (minutes < 1) return "just now";
  if (minutes === 1) return "1 min ago";
  return `${minutes} min ago`;
};

// Load favourites
const loadFavourites = async () => {
  if (!isAuthenticated.value) return;

  try {
    const response = await favouritesService.getFavourites();
    if (response.success) {
      favourites.value = response.data || [];
      for (const fav of favourites.value) {
        await loadStationArrivals(fav.stopId);
      }
    }
  } catch (error) {
    console.error("Failed to load favourites:", error);
  }
};

// Load arrivals for a station
const loadStationArrivals = async (stopId) => {
  try {
    const data = await arrivalsService.getArrivals(stopId);
    arrivalsData.value[stopId] = data.arrivals || [];
  } catch (error) {
    console.error(`Failed to load arrivals for ${stopId}:`, error);
  }
};

// Load alerts
const loadAlerts = async () => {
  try {
    const response = await alertsService.getAllAlerts();
    if (response.success) {
      alerts.value = response.data || [];
    }
  } catch (error) {
    console.error("Failed to load alerts:", error);
  }
};

// Refresh all data
const refreshData = () => {
  loadFavourites();
  loadAlerts();
  showStaleBanner.value = false;
};

// Navigation
const goToStation = (stopId) => {
  router.push(`/station/${stopId}`);
};

const goToSearch = () => {
  router.push("/search");
};

const goToFavourites = () => {
  router.push("/favourites");
};

const goToAlerts = () => {
  router.push("/alerts");
};

const goToAlert = (alertId) => {
  router.push(`/alerts/${alertId}`);
};

const goToProfile = () => {
  router.push("/profile");
};

const goToMap = () => {
  router.push("/map");
};

const goToLogin = () => {
  router.push("/login");
};

// Start countdown timer
const startCountdownTimer = () => {
  if (countdownInterval) clearInterval(countdownInterval);
  countdownInterval = setInterval(() => {
    arrivalsData.value = { ...arrivalsData.value };
  }, 1000);
};

onMounted(async () => {
  await loadFavourites();
  await loadAlerts();
  startCountdownTimer();
});

onUnmounted(() => {
  if (countdownInterval) clearInterval(countdownInterval);
  if (staleInterval) clearInterval(staleInterval);
});
</script>
