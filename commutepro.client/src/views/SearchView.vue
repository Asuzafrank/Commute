<!-- frontend/src/views/SearchView.vue -->
<template>
  <div class="min-h-screen" style="background: #243540">
    <!-- Search Header -->
    <div
      class="sticky top-0 z-40 px-4 pt-4 pb-3"
      style="background: #243540; border-bottom: 1px solid rgba(255, 255, 255, 0.07)"
    >
      <div
        class="flex items-center gap-3 rounded-xl px-3"
        style="background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.08)"
      >
        <Search
          size="18"
          :color="searchQuery ? 'rgba(255,255,255,0.7)' : '#4A6478'"
          class="flex-shrink-0"
        />
        <input
          ref="searchInput"
          v-model="searchQuery"
          type="text"
          placeholder="Search for a station…"
          class="flex-1 bg-transparent border-none outline-none text-white text-sm font-manrope py-3.5"
          style="caret-color: #ffffff"
          @input="onSearchInput"
        />
        <button
          v-if="searchQuery"
          @click="clearSearch"
          class="bg-transparent border-none cursor-pointer p-1"
        >
          <X size="16" color="#4A6478" />
        </button>
      </div>
    </div>

    <!-- Recent Searches (when no search query) -->
    <Transition name="fade" mode="out-in">
      <div v-if="!searchQuery" key="recent" class="pb-4">
        <!-- Recent Searches Header -->
        <div class="px-5 pt-5 pb-3">
          <h3 class="text-gray-500 text-[11px] font-semibold font-manrope tracking-wide uppercase">
            Recent Searches
          </h3>
        </div>

        <!-- Recent Searches List -->
        <TransitionGroup name="list" tag="div">
          <div
            v-for="recent in recentSearches"
            :key="recent"
            class="w-full flex items-center justify-between px-5 py-3 border-b border-white/5"
          >
            <button
              @click="
                searchQuery = recent;
                performSearch();
              "
              class="flex-1 flex items-center gap-3 text-left cursor-pointer bg-transparent border-none"
            >
              <div
                class="w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0"
                style="background: rgba(255, 255, 255, 0.05)"
              >
                <Clock size="14" color="#4A6478" />
              </div>
              <div class="flex-1">
                <span class="text-white text-sm font-manrope">{{ recent }}</span>
                <div class="flex items-center gap-1 mt-1">
                  <span class="text-gray-500 text-[11px] font-manrope">Station</span>
                </div>
              </div>
            </button>
            <button
              @click.stop="removeRecentSearch(recent)"
              class="bg-transparent border-none cursor-pointer p-1 flex-shrink-0"
            >
              <X size="14" color="#4A6478" />
            </button>
          </div>
        </TransitionGroup>

        <!-- All Stations Header -->
        <div class="px-5 pt-5 pb-3">
          <h3 class="text-gray-500 text-[11px] font-semibold font-manrope tracking-wide uppercase">
            All Stations
          </h3>
        </div>

        <!-- All Stations List -->
        <TransitionGroup name="list" tag="div">
          <button
            v-for="station in allStations"
            :key="station.stopId"
            @click="goToStation(station.stopId)"
            class="w-full flex items-center gap-3 px-5 py-3 text-left cursor-pointer transition-all hover:bg-white/5"
            style="border-bottom: 1px solid rgba(255, 255, 255, 0.05)"
          >
            <div
              class="w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0"
              style="
                background: rgba(255, 255, 255, 0.05);
                border: 1px solid rgba(255, 255, 255, 0.08);
              "
            >
              <MapPin size="14" color="rgba(255,255,255,0.5)" />
            </div>
            <div class="flex-1 min-w-0">
              <div class="text-white text-sm font-manrope truncate">
                {{ station.stopName }}
              </div>
              <div class="flex items-center gap-1 flex-wrap mt-1">
                <span class="text-gray-500 text-[11px] font-manrope">Station</span>
                <span class="text-white/10 mx-1">·</span>
                <span class="text-gray-500 text-[11px] font-manrope">ID: {{ station.stopId }}</span>
              </div>
            </div>
            <ChevronRight size="16" color="rgba(255,255,255,0.2)" />
          </button>
        </TransitionGroup>
      </div>

      <!-- Search Results -->
      <div v-else key="results" class="pb-4">
        <!-- Results Header -->
        <div class="px-5 pt-4 pb-2">
          <span class="text-gray-500 text-xs font-manrope">
            {{ filteredStations.length }} result{{ filteredStations.length !== 1 ? "s" : "" }} for
            "{{ searchQuery }}"
          </span>
        </div>

        <!-- No Results -->
        <div
          v-if="filteredStations.length === 0 && !loading && searched"
          class="flex flex-col items-center justify-center px-8 py-16 text-center"
        >
          <div
            class="w-15 h-15 rounded-full flex items-center justify-center mb-4"
            style="background: rgba(255, 255, 255, 0.05)"
          >
            <Search size="28" color="#4A6478" />
          </div>
          <p class="text-white text-base font-semibold font-manrope mb-2">
            No stations found for "{{ searchQuery }}"
          </p>
          <p class="text-gray-500 text-xs font-manrope">
            Check the spelling or try a different name
          </p>
        </div>

        <!-- Loading State -->
        <div v-else-if="loading" class="flex justify-center py-12">
          <div
            class="animate-spin rounded-full h-8 w-8 border-2 border-orange-500 border-t-transparent"
          ></div>
        </div>

        <!-- Results List -->
        <TransitionGroup v-else name="list" tag="div">
          <button
            v-for="station in filteredStations"
            :key="station.stopId"
            @click="goToStation(station.stopId)"
            class="w-full flex items-center gap-3 px-5 py-4 text-left cursor-pointer transition-all hover:bg-white/5"
            style="border-bottom: 1px solid rgba(255, 255, 255, 0.05)"
          >
            <div
              class="w-9 h-9 rounded-full flex items-center justify-center flex-shrink-0"
              style="
                background: rgba(255, 255, 255, 0.05);
                border: 1px solid rgba(255, 255, 255, 0.08);
              "
            >
              <MapPin size="16" color="rgba(255,255,255,0.5)" />
            </div>
            <div class="flex-1 min-w-0">
              <div
                class="text-white text-sm font-manrope mb-1"
                v-html="highlightText(station.stopName)"
              ></div>
              <div class="flex items-center gap-1 flex-wrap">
                <span class="text-gray-500 text-[11px] font-manrope"
                  >Station ID: {{ station.stopId }}</span
                >
              </div>
            </div>
            <ChevronRight size="16" color="rgba(255,255,255,0.2)" />
          </button>
        </TransitionGroup>
      </div>
    </Transition>

    <div class="h-6"></div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import { useRouter } from "vue-router";
import { Search, X, Clock, MapPin, ChevronRight } from "lucide-vue-next";
import stationsService from "@/services/api/stations.service";

const router = useRouter();

// State
const searchQuery = ref("");
const searchInput = ref(null);
const allStationsList = ref([]);
const filteredStations = ref([]);
const loading = ref(false);
const searched = ref(false);
const recentSearches = ref([]);

// Computed
const allStations = computed(() => {
  return allStationsList.value.slice(0, 50);
});

// Load recent searches from localStorage
const loadRecentSearches = () => {
  const saved = localStorage.getItem("recentStationSearches");
  if (saved) {
    recentSearches.value = JSON.parse(saved);
  }
};

// Save recent search
const saveRecentSearch = (query) => {
  if (!query || query.length < 2) return;

  // Remove if exists
  const index = recentSearches.value.indexOf(query);
  if (index !== -1) {
    recentSearches.value.splice(index, 1);
  }

  // Add to front
  recentSearches.value.unshift(query);

  // Keep only last 5
  if (recentSearches.value.length > 5) {
    recentSearches.value = recentSearches.value.slice(0, 5);
  }

  localStorage.setItem("recentStationSearches", JSON.stringify(recentSearches.value));
};

// Remove recent search
const removeRecentSearch = (query) => {
  recentSearches.value = recentSearches.value.filter((s) => s !== query);
  localStorage.setItem("recentStationSearches", JSON.stringify(recentSearches.value));
};

// Load all stations
const loadAllStations = async () => {
  try {
    // Search for a common letter to get a list of stations
    const response = await stationsService.search("a", 50);
    if (response.success) {
      allStationsList.value = response.data || [];
    }
  } catch (error) {
    console.error("Failed to load stations:", error);
    allStationsList.value = [];
  }
};

// Perform search
const performSearch = async () => {
  if (!searchQuery.value || searchQuery.value.length < 2) {
    filteredStations.value = [];
    return;
  }

  loading.value = true;
  searched.value = true;

  try {
    const response = await stationsService.search(searchQuery.value);
    if (response.success) {
      filteredStations.value = response.data || [];
      if (filteredStations.value.length > 0) {
        saveRecentSearch(searchQuery.value);
      }
    }
  } catch (error) {
    console.error("Search failed:", error);
    filteredStations.value = [];
  } finally {
    loading.value = false;
  }
};

// Debounced search
let debounceTimer;
const onSearchInput = () => {
  clearTimeout(debounceTimer);
  if (searchQuery.value.length >= 2) {
    debounceTimer = setTimeout(() => {
      performSearch();
    }, 300);
  } else if (searchQuery.value.length === 0) {
    filteredStations.value = [];
    searched.value = false;
  }
};

// Clear search
const clearSearch = () => {
  searchQuery.value = "";
  filteredStations.value = [];
  searched.value = false;
  searchInput.value?.focus();
};

// Go to station details
const goToStation = (stopId) => {
  router.push(`/station/${stopId}`);
};

// Highlight matching text
const highlightText = (text) => {
  if (!searchQuery.value || searchQuery.value.length < 2) return text;
  const regex = new RegExp(`(${searchQuery.value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")})`, "gi");
  return text.replace(regex, '<span class="text-white font-bold">$1</span>');
};

// Focus input on mount
onMounted(() => {
  loadAllStations();
  loadRecentSearches();
  setTimeout(() => {
    searchInput.value?.focus();
  }, 100);
});
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.15s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

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
</style>
