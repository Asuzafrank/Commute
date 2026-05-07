<!-- frontend/src/views/AlertsView.vue -->
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
        <div>
          <h1 class="text-white text-xl font-bold font-manrope">Alerts</h1>
          <p class="text-gray-500 text-xs font-manrope mt-0.5">{{ unreadCount }} unread</p>
        </div>
        <div class="flex items-center gap-2">
          <button
            @click="markAllAsRead"
            class="px-3 py-1.5 rounded-lg text-[11px] font-semibold font-manrope transition-all"
            style="
              background: none;
              border: 1px solid rgba(255, 255, 255, 0.1);
              color: rgba(255, 255, 255, 0.5);
            "
          >
            Mark all read
          </button>
          <button
            @click="refreshAlerts"
            class="w-9 h-9 rounded-lg flex items-center justify-center cursor-pointer transition-all hover:bg-white/5"
            style="background: rgba(255, 255, 255, 0.05); border: none"
          >
            <Filter size="16" color="#94A3B8" />
          </button>
        </div>
      </div>
    </div>

    <!-- Filter Chips -->
    <div
      class="flex items-center gap-2 px-4 py-3 overflow-x-auto"
      style="scrollbar-width: none; border-bottom: 1px solid rgba(255, 255, 255, 0.07)"
    >
      <button
        v-for="filter in filters"
        :key="filter.key"
        @click="activeFilter = filter.key"
        class="px-3.5 py-1.5 rounded-full text-xs font-semibold whitespace-nowrap transition-all"
        :class="
          activeFilter === filter.key
            ? 'bg-white/10 text-white border-white/30'
            : 'bg-transparent text-gray-500 border-white/10'
        "
        style="border-width: 1px"
      >
        {{ filter.label }}
      </button>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex justify-center py-12">
      <div
        class="animate-spin rounded-full h-10 w-10 border-2 border-orange-500 border-t-transparent"
      ></div>
    </div>

    <!-- Empty State -->
    <div
      v-else-if="filteredAlerts.length === 0"
      class="flex flex-col items-center justify-center px-8 py-20 text-center"
    >
      <div
        class="w-18 h-18 rounded-full flex items-center justify-center mb-5"
        style="background: rgba(34, 197, 94, 0.1); border: 1px solid rgba(34, 197, 94, 0.2)"
      >
        <CheckCircle size="32" color="#22C55E" />
      </div>
      <h3 class="text-white text-lg font-semibold font-manrope mb-2">All clear</h3>
      <p class="text-gray-400 text-sm font-manrope leading-relaxed">
        No active alerts for your lines. All services running normally.
      </p>
    </div>

    <!-- Alerts List -->
    <div v-else class="px-4 py-4 space-y-3">
      <TransitionGroup name="alert">
        <div
          v-for="alert in filteredAlerts"
          :key="alert.id"
          class="rounded-xl overflow-hidden transition-all"
          :style="{
            background: '#2C3E4A',
            border: '1px solid rgba(255,255,255,0.07)',
            borderLeft: `3px solid ${getAlertColor(alert)}`,
            opacity: alert.isRead ? 0.75 : 1,
            boxShadow: '0 4px 20px rgba(0,0,0,0.2)',
          }"
        >
          <button
            @click="toggleExpand(alert.id)"
            class="w-full text-left bg-transparent border-none cursor-pointer p-3.5"
          >
            <div class="flex items-start gap-3">
              <!-- Icon -->
              <div
                class="w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0"
                :style="{ background: getAlertBgColor(alert) }"
              >
                <component :is="getAlertIcon(alert)" size="16" :color="getAlertColor(alert)" />
              </div>

              <div class="flex-1 min-w-0">
                <!-- Badges -->
                <div class="flex items-center gap-2 mb-1 flex-wrap">
                  <span
                    class="text-[9px] font-bold px-1.5 py-0.5 rounded-full uppercase tracking-wide"
                    :style="{ background: getAlertBgColor(alert), color: getAlertColor(alert) }"
                  >
                    {{ getAlertTypeLabel(alert) }}
                  </span>
                  <LineChip
                    v-for="route in alert.affectedRoutes.slice(0, 2)"
                    :key="route"
                    :lineId="route"
                    size="xs"
                  />
                  <span
                    v-if="!alert.isRead"
                    class="w-1.5 h-1.5 rounded-full bg-white/50 ml-auto flex-shrink-0"
                  ></span>
                </div>

                <!-- Title -->
                <p class="text-white text-sm font-semibold font-manrope mb-1 leading-relaxed">
                  {{ truncateText(alert.headerText, 80) }}
                </p>

                <!-- Footer -->
                <div class="flex items-center justify-between mt-2">
                  <div class="flex items-center gap-1">
                    <span class="text-gray-500 text-[11px] font-manrope">
                      {{ alert.affectedStops.slice(0, 2).join(" · ") || "System-wide" }}
                    </span>
                  </div>
                  <div class="flex items-center gap-1">
                    <span class="text-gray-500 text-[11px] font-manrope">
                      {{ formatTimeAgo(alert.startTime) }}
                    </span>
                    <ChevronDown
                      size="14"
                      :class="expandedId === alert.id ? 'rotate-180' : ''"
                      class="transition-transform duration-200"
                      color="#4A6478"
                    />
                  </div>
                </div>
              </div>
            </div>
          </button>

          <!-- Expanded Details -->
          <Transition name="expand">
            <div v-if="expandedId === alert.id" class="overflow-hidden">
              <div
                class="pt-0 pb-3.5 px-3.5"
                style="border-top: 1px solid rgba(255, 255, 255, 0.07)"
              >
                <p class="text-gray-400 text-xs font-manrope leading-relaxed mb-3">
                  {{ alert.descriptionText || alert.headerText }}
                </p>
                <div class="flex flex-wrap gap-2">
                  <button
                    v-if="alert.affectedStops.length > 0"
                    @click.stop="goToStation(alert.affectedStops[0])"
                    class="px-3 py-1.5 rounded-lg text-[11px] font-semibold transition-all"
                    style="
                      background: rgba(255, 255, 255, 0.06);
                      border: 1px solid rgba(255, 255, 255, 0.12);
                      color: rgba(255, 255, 255, 0.65);
                    "
                  >
                    View affected stations
                  </button>
                  <a
                    v-if="alert.url"
                    :href="alert.url"
                    target="_blank"
                    rel="noopener noreferrer"
                    class="px-3 py-1.5 rounded-lg text-[11px] font-semibold transition-all no-underline"
                    style="
                      background: rgba(255, 255, 255, 0.06);
                      border: 1px solid rgba(255, 255, 255, 0.12);
                      color: rgba(255, 255, 255, 0.65);
                    "
                  >
                    More info
                  </a>
                </div>
              </div>
            </div>
          </Transition>
        </div>
      </TransitionGroup>
    </div>

    <div class="h-6"></div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import { useToast } from "vue-toastification";
import { Filter, ChevronDown, AlertTriangle, Bell, CheckCircle } from "lucide-vue-next";
import alertsService from "@/services/api/alerts.service";
import LineChip from "@/components/LineChip.vue";

const router = useRouter();
const toast = useToast();

// State
const alerts = ref([]);
const loading = ref(true);
const activeFilter = ref("all");
const expandedId = ref(null);
let refreshInterval = null;

// Filters
const filters = [
  { key: "all", label: "All" },
  { key: "delays", label: "Delays" },
  { key: "service-changes", label: "Service Changes" },
];

// Computed
const unreadCount = computed(() => {
  return alerts.value.filter((a) => !a.isRead).length;
});

const filteredAlerts = computed(() => {
  let filtered = [...alerts.value];

  if (activeFilter.value === "delays") {
    filtered = filtered.filter((a) => a.effect === "Significant Delays" || a.effect === "Delay");
  } else if (activeFilter.value === "service-changes") {
    filtered = filtered.filter(
      (a) => a.effect === "No Service" || a.effect === "Reduced Service" || a.effect === "Detour",
    );
  }

  // Sort by severity (SEVERE first, then WARNING, then INFO)
  const severityOrder = { SEVERE: 0, WARNING: 1, INFO: 2 };
  filtered.sort((a, b) => severityOrder[a.severity] - severityOrder[b.severity]);

  return filtered;
});

// Methods
const getAlertColor = (alert) => {
  if (alert.severity === "SEVERE") return "#E63946";
  if (alert.severity === "WARNING") return "#F5A623";
  return "#009FE0";
};

const getAlertBgColor = (alert) => {
  if (alert.severity === "SEVERE") return "rgba(230,57,70,0.1)";
  if (alert.severity === "WARNING") return "rgba(245,166,35,0.1)";
  return "rgba(0,159,224,0.1)";
};

const getAlertIcon = (alert) => {
  if (alert.effect === "No Service" || alert.effect === "Cancellation") return AlertTriangle;
  return Bell;
};

const getAlertTypeLabel = (alert) => {
  if (alert.effect === "No Service") return "No Service";
  if (alert.effect === "Significant Delays") return "Delay";
  if (alert.effect === "Cancellation") return "Cancellation";
  if (alert.effect === "Reduced Service") return "Reduced Service";
  if (alert.effect === "Detour") return "Detour";
  return "Service Change";
};

const truncateText = (text, maxLength) => {
  if (!text) return "";
  if (text.length <= maxLength) return text;
  return text.substring(0, maxLength) + "…";
};

const formatTimeAgo = (dateString) => {
  if (!dateString) return "recent";
  const date = new Date(dateString);
  const minutes = Math.floor((Date.now() - date.getTime()) / 60000);
  if (minutes < 1) return "just now";
  if (minutes === 1) return "1 min ago";
  if (minutes < 60) return `${minutes} min ago`;
  const hours = Math.floor(minutes / 60);
  if (hours === 1) return "1 hour ago";
  return `${hours} hours ago`;
};

const toggleExpand = (id) => {
  if (expandedId.value === id) {
    expandedId.value = null;
  } else {
    expandedId.value = id;
    // Mark as read when expanded
    markAsRead(id);
  }
};

const markAsRead = async (alertId) => {
  const alert = alerts.value.find((a) => a.id === alertId);
  if (alert && !alert.isRead) {
    alert.isRead = true;
    // Optional: call API to mark as read
  }
};

const markAllAsRead = () => {
  alerts.value.forEach((alert) => {
    alert.isRead = true;
  });
  toast.success("All alerts marked as read");
};

const goToStation = (stopId) => {
  router.push(`/station/${stopId}`);
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
    toast.error("Failed to load service alerts");
  } finally {
    loading.value = false;
  }
};

const refreshAlerts = async () => {
  loading.value = true;
  await loadAlerts();
  toast.success("Alerts refreshed");
};

// Auto-refresh every 60 seconds
const startAutoRefresh = () => {
  refreshInterval = setInterval(() => {
    loadAlerts();
  }, 60000);
};

onMounted(() => {
  loadAlerts();
  startAutoRefresh();
});

onUnmounted(() => {
  if (refreshInterval) clearInterval(refreshInterval);
});
</script>

<style scoped>
.alert-enter-active,
.alert-leave-active {
  transition: all 0.2s ease;
}

.alert-enter-from {
  opacity: 0;
  transform: translateY(20px);
}

.alert-leave-to {
  opacity: 0;
  transform: translateY(-20px);
}

.expand-enter-active,
.expand-leave-active {
  transition: all 0.2s ease-out;
  overflow: hidden;
}

.expand-enter-from,
.expand-leave-to {
  opacity: 0;
  max-height: 0;
}

.expand-enter-to,
.expand-leave-from {
  opacity: 1;
  max-height: 300px;
}

.rotate-180 {
  transform: rotate(180deg);
}
</style>
