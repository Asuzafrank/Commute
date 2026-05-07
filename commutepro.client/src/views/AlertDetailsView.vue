<!-- frontend/src/views/AlertDetailsView.vue -->
<template>
  <div class="min-h-screen" style="background: #243540">
    <!-- Loading State -->
    <div v-if="loading" class="flex justify-center py-12">
      <div
        class="animate-spin rounded-full h-10 w-10 border-2 border-orange-500 border-t-transparent"
      ></div>
    </div>

    <!-- Alert Content -->
    <div v-else-if="alert" class="space-y-4">
      <!-- Back Button -->
      <div class="sticky top-0 z-40 px-4 pt-4 pb-2" style="background: #243540">
        <button
          @click="goBack"
          class="flex items-center gap-2 text-gray-400 hover:text-white transition-colors"
        >
          <ChevronLeft size="20" />
          <span class="text-sm font-manrope">Back to Alerts</span>
        </button>
      </div>

      <!-- Alert Card -->
      <div
        class="mx-4 rounded-xl overflow-hidden shadow-lg"
        style="background: #2c3e4a; border: 1px solid rgba(255, 255, 255, 0.07)"
      >
        <!-- Header Banner based on severity -->
        <div
          class="p-5"
          :style="{
            background: getSeverityBgColor(),
            borderLeft: `4px solid ${getSeverityColor()}`,
          }"
        >
          <div class="flex flex-wrap gap-2 mb-3">
            <span
              class="px-3 py-1 rounded-full text-xs font-semibold"
              :style="{
                background: getSeverityBgColor(),
                color: getSeverityColor(),
                border: `1px solid ${getSeverityColor()}30`,
              }"
            >
              {{ alert.severity }}
            </span>
            <span
              class="px-3 py-1 rounded-full text-xs font-semibold"
              style="background: rgba(255, 255, 255, 0.1); color: #94a3b8"
            >
              {{ alert.effect }}
            </span>
            <span
              v-if="alert.cause"
              class="px-3 py-1 rounded-full text-xs font-medium"
              style="background: rgba(255, 255, 255, 0.05); color: #6b8a96"
            >
              {{ alert.cause }}
            </span>
          </div>

          <h1 class="text-white text-xl font-bold font-manrope leading-relaxed">
            {{ alert.headerText }}
          </h1>
        </div>

        <div class="p-5 space-y-5">
          <!-- Description -->
          <div v-if="alert.descriptionText">
            <h3
              class="text-gray-400 text-xs font-semibold font-manrope uppercase tracking-wide mb-2"
            >
              Details
            </h3>
            <p class="text-gray-300 text-sm font-manrope leading-relaxed whitespace-pre-line">
              {{ alert.descriptionText }}
            </p>
          </div>

          <!-- Affected Routes -->
          <div v-if="alert.affectedRoutes.length > 0">
            <h3
              class="text-gray-400 text-xs font-semibold font-manrope uppercase tracking-wide mb-2"
            >
              Affected Routes
            </h3>
            <div class="flex flex-wrap gap-2">
              <LineChip
                v-for="route in alert.affectedRoutes"
                :key="route"
                :lineId="route"
                size="sm"
              />
            </div>
          </div>

          <!-- Affected Stops -->
          <div v-if="alert.affectedStops.length > 0">
            <h3
              class="text-gray-400 text-xs font-semibold font-manrope uppercase tracking-wide mb-2"
            >
              Affected Stops
            </h3>
            <div class="flex flex-wrap gap-2">
              <button
                v-for="stop in alert.affectedStops.slice(0, 15)"
                :key="stop"
                @click="goToStation(stop)"
                class="px-3 py-1.5 rounded-full text-xs font-medium transition-all hover:bg-white/10"
                style="background: rgba(255, 255, 255, 0.05); color: #94a3b8"
              >
                {{ stop }}
              </button>
              <span
                v-if="alert.affectedStops.length > 15"
                class="text-gray-500 text-xs font-manrope self-center"
              >
                +{{ alert.affectedStops.length - 15 }} more
              </span>
            </div>
          </div>

          <!-- Time Information -->
          <div
            class="rounded-xl p-4"
            style="
              background: rgba(255, 255, 255, 0.03);
              border: 1px solid rgba(255, 255, 255, 0.05);
            "
          >
            <h3
              class="text-gray-400 text-xs font-semibold font-manrope uppercase tracking-wide mb-3"
            >
              Time Information
            </h3>
            <div class="space-y-2 text-sm">
              <div v-if="alert.startTime" class="flex items-center gap-2">
                <Clock size="14" color="#6B8A96" />
                <span class="text-gray-300 font-manrope">Started:</span>
                <span class="text-gray-400 font-manrope">{{
                  formatDateTime(alert.startTime)
                }}</span>
              </div>
              <div v-if="alert.endTime" class="flex items-center gap-2">
                <Clock size="14" color="#6B8A96" />
                <span class="text-gray-300 font-manrope">Ends:</span>
                <span class="text-gray-400 font-manrope">{{ formatDateTime(alert.endTime) }}</span>
              </div>
              <div v-else class="flex items-center gap-2">
                <Clock size="14" color="#6B8A96" />
                <span class="text-gray-300 font-manrope">Until further notice</span>
              </div>
              <div class="flex items-center gap-2 pt-1">
                <div
                  class="w-2 h-2 rounded-full"
                  :class="alert.isActive ? 'bg-green-500' : 'bg-gray-500'"
                ></div>
                <span class="text-gray-300 font-manrope">Status:</span>
                <span
                  :class="alert.isActive ? 'text-green-400' : 'text-gray-500'"
                  class="font-manrope"
                >
                  {{ alert.isActive ? "Active" : "Expired" }}
                </span>
              </div>
            </div>
          </div>

          <!-- Service Effect Text -->
          <div v-if="alert.serviceEffectText">
            <h3
              class="text-gray-400 text-xs font-semibold font-manrope uppercase tracking-wide mb-2"
            >
              Service Effect
            </h3>
            <p class="text-gray-300 text-sm font-manrope">
              {{ alert.serviceEffectText }}
            </p>
          </div>

          <!-- External Link -->
          <div v-if="alert.url">
            <a
              :href="alert.url"
              target="_blank"
              rel="noopener noreferrer"
              class="inline-flex items-center gap-2 text-orange-400 hover:text-orange-300 transition-colors text-sm font-manrope"
            >
              More Information
              <ExternalLink size="14" />
            </a>
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
        <AlertTriangle size="32" color="#6B8A96" />
      </div>
      <h3 class="text-white text-lg font-semibold font-manrope mb-2">Alert Not Found</h3>
      <p class="text-gray-400 text-sm font-manrope mb-6">
        The alert you're looking for doesn't exist or has expired.
      </p>
      <button
        @click="goBack"
        class="px-6 py-3 bg-orange-500 text-gray-900 rounded-xl font-semibold text-sm font-manrope hover:bg-orange-600 transition"
      >
        Back to Alerts
      </button>
    </div>

    <div class="h-6"></div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useToast } from "vue-toastification";
import { ChevronLeft, Clock, ExternalLink, AlertTriangle } from "lucide-vue-next";
import alertsService from "@/services/api/alerts.service";
import LineChip from "@/components/LineChip.vue";

const route = useRoute();
const router = useRouter();
const toast = useToast();

// State
const alertId = ref(route.params.alertId);
const alert = ref(null);
const loading = ref(true);

// Methods
const getSeverityColor = () => {
  if (!alert.value) return "#009FE0";
  if (alert.value.severity === "SEVERE") return "#E63946";
  if (alert.value.severity === "WARNING") return "#F5A623";
  return "#009FE0";
};

const getSeverityBgColor = () => {
  if (!alert.value) return "rgba(0,159,224,0.1)";
  if (alert.value.severity === "SEVERE") return "rgba(230,57,70,0.08)";
  if (alert.value.severity === "WARNING") return "rgba(245,166,35,0.08)";
  return "rgba(0,159,224,0.08)";
};

const formatDateTime = (dateString) => {
  if (!dateString) return "";
  const date = new Date(dateString);
  return date.toLocaleString([], {
    weekday: "short",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
};

const goBack = () => {
  router.push("/alerts");
};

const goToStation = (stopId) => {
  const isTripId = /^\d{8,}$/.test(stopId);

  if (isTripId) {
    // It's actually a trip ID - go to trip page
    router.push(`/trip/${stopId}`);
  } else {
    // It's a real stop ID - go to station page
    router.push(`/station/${stopId}`);
  }
};

// Load alert
const loadAlert = async () => {
  loading.value = true;
  try {
    const response = await alertsService.getAlertById(alertId.value);
    if (response.success) {
      alert.value = response.data;
    } else {
      alert.value = null;
    }
  } catch (error) {
    console.error("Failed to load alert:", error);
    toast.error("Failed to load alert details");
    alert.value = null;
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  loadAlert();
});
</script>

<style scoped>
/* Smooth fade-in animation for alert content */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
