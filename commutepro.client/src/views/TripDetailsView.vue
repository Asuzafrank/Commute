<!-- frontend/src/views/TripDetailsView.vue -->
<template>
  <div class="min-h-screen" style="background: #243540">
    <!-- Loading State -->
    <div v-if="loading" class="flex justify-center py-12">
      <div
        class="animate-spin rounded-full h-10 w-10 border-2 border-orange-500 border-t-transparent"
      ></div>
    </div>

    <!-- Trip Details -->
    <div v-else-if="tripData" class="pb-6">
      <!-- Header -->
      <div class="sticky top-0 z-40 px-4 pt-4 pb-2" style="background: #243540">
        <div class="flex items-center gap-3">
          <button
            @click="goBack"
            class="w-9 h-9 rounded-lg flex items-center justify-center cursor-pointer transition-all hover:bg-white/5"
            style="background: rgba(255, 255, 255, 0.05); border: none"
          >
            <ChevronLeft size="20" color="rgba(255,255,255,0.75)" />
          </button>
          <div class="flex-1">
            <h1 class="text-white text-lg font-bold font-manrope">{{ tripData.lineName }} line</h1>
            <p class="text-gray-400 text-xs font-manrope">to {{ tripData.destination }}</p>
          </div>
          <StatusPill :status="tripData.status" />
        </div>
      </div>

      <!-- Trip Info Bar -->
      <div
        class="flex items-center gap-3 px-4 py-4"
        style="border-bottom: 1px solid rgba(255, 255, 255, 0.07)"
      >
        <div
          class="px-3 py-1 rounded-full text-xs font-bold font-manrope"
          :style="{ background: tripData.lineColor, color: tripData.lineTextColor }"
        >
          {{ tripData.lineName }}
        </div>
        <span class="text-gray-400 text-xs font-manrope">to {{ tripData.destination }}</span>
        <span
          class="ml-auto px-2 py-1 rounded text-[10px] font-semibold font-manrope"
          style="background: #3a5060; color: #94a3b8"
        >
          #{{ tripData.tripId.slice(-6) }}
        </span>
      </div>

      <!-- Live Position Progress Bar -->
      <div
        v-if="currentStopIndex >= 0 && currentStopIndex < stops.length - 1"
        class="px-4 py-5"
        style="border-bottom: 1px solid rgba(255, 255, 255, 0.07)"
      >
        <div class="flex items-center justify-between mb-2">
          <span class="text-gray-400 text-[11px] font-manrope">{{
            stops[currentStopIndex]?.stopName
          }}</span>
          <span class="text-gray-400 text-[11px] font-manrope">{{
            stops[currentStopIndex + 1]?.stopName
          }}</span>
        </div>
        <div
          class="relative h-1.5 rounded-full overflow-hidden"
          style="background: rgba(255, 255, 255, 0.06)"
        >
          <div
            class="h-full rounded-full transition-all duration-1000 ease-linear"
            :style="{ width: `${progress}%`, background: tripData.lineColor }"
          ></div>
          <div
            class="absolute top-1/2 -translate-y-1/2 w-3 h-3 rounded-full bg-white shadow-lg"
            :style="{ left: `calc(${progress}% - 6px)`, border: `2px solid ${tripData.lineColor}` }"
          ></div>
        </div>
        <p class="text-gray-500 text-[10px] font-manrope mt-2">Live position — between stops</p>
      </div>

      <!-- Route Timeline -->
      <div class="px-4 py-5">
        <h3 class="text-gray-500 text-[11px] font-bold font-manrope tracking-wide uppercase mb-4">
          Route
        </h3>

        <div class="relative">
          <!-- Vertical connecting line -->
          <div
            class="absolute left-3 top-3 bottom-3 w-0.5"
            :style="{
              background:
                'linear-gradient(180deg, rgba(255,255,255,0.15) 0%, rgba(255,255,255,0.04) 100%)',
            }"
          ></div>

          <!-- Stops -->
          <div
            v-for="(stop, index) in stops"
            :key="stop.stopId"
            class="relative flex items-start gap-4 pb-6"
          >
            <!-- Stop dot -->
            <div
              class="w-6 h-6 rounded-full flex items-center justify-center flex-shrink-0 z-10 transition-all"
              :class="stop.isSkipped ? 'opacity-50' : ''"
              :style="getStopDotStyle(stop.status)"
            >
              <CheckCircle
                v-if="stop.status === 'past'"
                size="12"
                color="#6B8A96"
                stroke-width="2.5"
              />
              <Circle v-else-if="stop.status === 'current'" size="8" fill="#243540" />
              <Circle v-else size="8" fill="rgba(255,255,255,0.1)" stroke="rgba(255,255,255,0.1)" />
            </div>

            <!-- Stop info -->
            <div class="flex-1 flex items-start justify-between pt-0.5">
              <div>
                <span
                  class="block font-manrope transition-all"
                  :class="{
                    'text-white text-sm font-bold': stop.status === 'current',
                    'text-gray-400 text-sm font-medium':
                      stop.status === 'future' && !stop.isSkipped,
                    'text-gray-500 text-sm line-through': stop.status === 'past' || stop.isSkipped,
                  }"
                >
                  {{ stop.stopName }}
                  <span v-if="stop.isSkipped" class="text-red-400 text-[10px] font-semibold ml-2"
                    >(Skipped)</span
                  >
                </span>
                <span
                  v-if="stop.status === 'current'"
                  class="text-orange-500 text-[10px] font-semibold font-manrope tracking-wide mt-0.5 block"
                >
                  ● Current stop
                </span>
              </div>

              <!-- Times -->
              <div class="text-right">
                <div v-if="stop.liveTime" class="flex flex-col items-end">
                  <span class="text-gray-500 text-[11px] font-mono line-through">{{
                    stop.scheduledTime
                  }}</span>
                  <span class="text-amber-400 text-xs font-mono font-bold">{{
                    stop.liveTime
                  }}</span>
                </div>
                <span
                  v-else
                  class="text-sm font-mono transition-all"
                  :class="{
                    'text-white font-bold': stop.status === 'current',
                    'text-gray-400': stop.status === 'future',
                    'text-gray-500 line-through': stop.status === 'past',
                  }"
                >
                  {{ stop.scheduledTime }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Delay Alert Banner -->
      <div
        v-if="tripData.status === 'delayed'"
        class="mx-4 mb-6 rounded-xl p-4"
        style="background: rgba(245, 166, 35, 0.1); border: 1px solid rgba(245, 166, 35, 0.3)"
      >
        <div class="flex items-start gap-3">
          <AlertTriangle size="18" color="#F5A623" class="flex-shrink-0 mt-0.5" />
          <div>
            <p class="text-amber-400 text-xs font-bold font-manrope mb-1">
              Service Delayed · +{{ tripData.delayMinutes }} minutes
            </p>
            <p class="text-gray-400 text-[11px] font-manrope leading-relaxed">
              This service is running approximately {{ tripData.delayMinutes }} minutes late. New
              ETA at {{ tripData.destination }}: adjusted accordingly.
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Not Found -->
    <div v-else class="flex flex-col items-center justify-center px-8 py-20 text-center">
      <div
        class="w-18 h-18 rounded-full flex items-center justify-center mb-5"
        style="background: rgba(107, 138, 150, 0.2)"
      >
        <Train size="32" color="#6B8A96" />
      </div>
      <h3 class="text-white text-lg font-semibold font-manrope mb-2">Trip Not Found</h3>
      <p class="text-gray-400 text-sm font-manrope mb-6">
        The trip you're looking for doesn't exist or has completed its journey.
      </p>
      <button
        @click="goBack"
        class="px-6 py-3 bg-orange-500 text-gray-900 rounded-xl font-semibold text-sm font-manrope hover:bg-orange-600 transition"
      >
        Go Back
      </button>
    </div>

    <div class="h-6"></div>
  </div>
</template>

<!-- frontend/src/views/TripDetailsView.vue -->
<script setup>
import { ref, onMounted, onUnmounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useToast } from "vue-toastification";
import { ChevronLeft, CheckCircle, Circle, AlertTriangle, Train } from "lucide-vue-next";
import StatusPill from "@/components/StatusPill.vue";
import tripsService from "@/services/api/trips.service";

const route = useRoute();
const router = useRouter();
const toast = useToast();

// State
const tripId = ref(route.params.tripId);
const tripData = ref(null);
const stops = ref([]);
const currentStopIndex = ref(-1);
const progress = ref(0);
const loading = ref(true);

let progressInterval = null;
let refreshInterval = null; // ← Add this for auto-refresh

// Get stop dot style based on status
const getStopDotStyle = (status) => {
  if (status === "current") {
    return {
      background: "rgba(255,255,255,0.9)",
      border: "2px solid rgba(255,255,255,0.4)",
      boxShadow: "0 0 12px rgba(255,255,255,0.2)",
    };
  }
  if (status === "past") {
    return {
      background: "rgba(255,255,255,0.08)",
      border: "2px solid rgba(255,255,255,0.1)",
    };
  }
  return {
    background: "rgba(255,255,255,0.04)",
    border: "2px solid rgba(255,255,255,0.1)",
  };
};

// Load trip details from API
const loadTripDetails = async () => {
  try {
    const response = await tripsService.getTripDetails(tripId.value);
    if (response.success) {
      const data = response.data;

      tripData.value = {
        tripId: data.tripId,
        lineName: data.routeShortName,
        lineColor: `#${data.routeColor}`,
        lineTextColor: `#${data.routeTextColor}`,
        destination: data.headsign,
        status: data.status === "Delayed" ? "delayed" : "live",
        delayMinutes: data.delayMinutes || 0,
      };

      stops.value = data.stops.map((stop) => ({
        stopId: stop.stopId,
        stopName: stop.stopName,
        scheduledTime: stop.scheduledTime,
        liveTime: stop.liveTime,
        status: stop.status,
        delaySeconds: stop.delaySeconds,
        isSkipped: stop.isSkipped || false,
      }));

      currentStopIndex.value = stops.value.findIndex((s) => s.status === "current");

      // Start progress animation if there's a current stop
      if (currentStopIndex.value >= 0 && currentStopIndex.value < stops.value.length - 1) {
        startProgressAnimation();
      }
    } else {
      tripData.value = null;
    }
  } catch (error) {
    console.error("Failed to load trip details:", error);
    tripData.value = null;
  } finally {
    loading.value = false;
  }
};

// Animate progress bar
const startProgressAnimation = () => {
  if (progressInterval) clearInterval(progressInterval);

  progress.value = 0.35;

  progressInterval = setInterval(() => {
    progress.value += 0.004;
    if (progress.value > 0.95) {
      progress.value = 0.35;
    }
  }, 1000);
};

// Auto-refresh every 15 seconds (realtime)
const startAutoRefresh = () => {
  if (refreshInterval) clearInterval(refreshInterval);
  refreshInterval = setInterval(() => {
    loadTripDetails();
  }, 15000);
};

const goBack = () => {
  router.back();
};

onMounted(() => {
  loadTripDetails();
  startAutoRefresh(); // ← Start auto-refresh
});

onUnmounted(() => {
  if (progressInterval) clearInterval(progressInterval);
  if (refreshInterval) clearInterval(refreshInterval); // ← Clean up
});
</script>

<style scoped>
/* Smooth transitions */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
