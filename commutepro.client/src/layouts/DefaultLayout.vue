<!-- frontend/src/layouts/DefaultLayout.vue -->
<template>
  <div class="min-h-screen flex flex-col" style="background: #243540">
    <!-- Desktop Navbar (hidden on mobile) -->
    <nav class="hidden md:block bg-[#1A2830] border-b border-[#3D5A68] sticky top-0 z-50">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center h-16">
          <!-- Logo -->
          <router-link to="/" class="flex items-center space-x-2">
            <img src="/src/assets/logo.png" alt="CommutePro" class="h-8 w-auto" />
            <span class="font-bold text-xl text-white hidden sm:inline font-manrope"
              >CommutePro</span
            >
          </router-link>

          <!-- Desktop Navigation -->
          <div class="flex items-center space-x-1">
            <router-link
              to="/"
              class="px-3 py-2 rounded-lg text-sm font-medium transition-all"
              :class="
                $route.path === '/'
                  ? 'text-white bg-white/10'
                  : 'text-gray-400 hover:text-white hover:bg-white/5'
              "
            >
              Home
            </router-link>
            <router-link
              to="/search"
              class="px-3 py-2 rounded-lg text-sm font-medium transition-all"
              :class="
                $route.path === '/search'
                  ? 'text-white bg-white/10'
                  : 'text-gray-400 hover:text-white hover:bg-white/5'
              "
            >
              Search
            </router-link>
            <router-link
              to="/alerts"
              class="px-3 py-2 rounded-lg text-sm font-medium transition-all relative"
              :class="
                $route.path === '/alerts'
                  ? 'text-white bg-white/10'
                  : 'text-gray-400 hover:text-white hover:bg-white/5'
              "
            >
              Alerts
              <span
                v-if="unreadAlertCount > 0"
                class="absolute -top-1 -right-1 w-4 h-4 bg-red-500 rounded-full text-[10px] text-white flex items-center justify-center"
              >
                {{ unreadAlertCount }}
              </span>
            </router-link>

            <!-- User Menu -->
            <div v-if="isAuthenticated" class="relative ml-2">
              <button
                @click="toggleDropdown"
                class="flex items-center gap-2 px-3 py-2 rounded-lg transition-all hover:bg-white/5"
              >
                <div class="w-7 h-7 rounded-full bg-orange-500 flex items-center justify-center">
                  <span class="text-white text-xs font-bold font-manrope">{{ userInitials }}</span>
                </div>
                <span class="text-gray-300 text-sm font-manrope hidden lg:inline">{{
                  userName
                }}</span>
                <ChevronDown size="14" class="text-gray-400" />
              </button>

              <div
                v-if="dropdownOpen"
                class="absolute right-0 mt-2 w-48 rounded-xl shadow-lg py-1 z-50"
                style="background: #354f5c; border: 1px solid #3d5a68"
              >
                <router-link
                  to="/profile"
                  class="block px-4 py-2 text-sm text-gray-300 hover:bg-white/10 transition-colors"
                >
                  Profile
                </router-link>
                <router-link
                  to="/favourites"
                  class="block px-4 py-2 text-sm text-gray-300 hover:bg-white/10 transition-colors"
                >
                  Favourites
                </router-link>
                <hr class="my-1 border-gray-600" />
                <button
                  @click="handleLogout"
                  class="block w-full text-left px-4 py-2 text-sm text-red-400 hover:bg-white/10 transition-colors"
                >
                  Logout
                </button>
              </div>
            </div>

            <router-link
              v-else
              to="/login"
              class="bg-orange-500 text-gray-900 px-4 py-2 rounded-lg text-sm font-medium hover:bg-orange-600 transition"
            >
              Login
            </router-link>
          </div>
        </div>
      </div>
    </nav>

    <!-- Main Content -->
    <main class="flex-1 max-w-7xl w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 pb-20 md:pb-6">
      <router-view />
    </main>

    <!-- Bottom Navigation (Mobile) -->
    <nav
      class="fixed bottom-0 left-0 right-0 md:hidden z-50"
      style="
        background: rgba(36, 53, 64, 0.95);
        backdropfilter: blur(20px);
        border-top: 1px solid #3d5a68;
      "
    >
      <div class="flex justify-around py-2">
        <router-link
          to="/"
          class="flex flex-col items-center py-2 px-4 rounded-xl transition-all"
          :class="$route.path === '/' ? 'text-white' : 'text-gray-500'"
        >
          <Home size="22" :stroke-width="$route.path === '/' ? 2.5 : 1.8" />
          <span class="text-[10px] font-medium mt-1 font-manrope">Home</span>
          <div
            v-if="$route.path === '/'"
            class="absolute -top-1 w-6 h-0.5 bg-white/70 rounded-full"
          ></div>
        </router-link>

        <router-link
          to="/search"
          class="flex flex-col items-center py-2 px-4 rounded-xl transition-all"
          :class="$route.path === '/search' ? 'text-white' : 'text-gray-500'"
        >
          <Search size="22" :stroke-width="$route.path === '/search' ? 2.5 : 1.8" />
          <span class="text-[10px] font-medium mt-1 font-manrope">Search</span>
        </router-link>
        <router-link
          to="/map"
          class="flex flex-col items-center py-2 px-4 rounded-xl transition-all"
          :class="$route.path === '/map' ? 'text-white' : 'text-gray-500'"
        >
          <Map size="22" :stroke-width="$route.path === '/map' ? 2.5 : 1.8" />
          <span class="text-[10px] font-medium mt-1 font-manrope">Map</span>
        </router-link>

        <router-link
          to="/alerts"
          class="flex flex-col items-center py-2 px-4 rounded-xl transition-all relative"
          :class="$route.path === '/alerts' ? 'text-white' : 'text-gray-500'"
        >
          <Bell size="22" :stroke-width="$route.path === '/alerts' ? 2.5 : 1.8" />
          <span
            v-if="unreadAlertCount > 0"
            class="absolute -top-1 right-3 w-4 h-4 bg-red-500 rounded-full text-[9px] text-white flex items-center justify-center"
          >
            {{ unreadAlertCount }}
          </span>
          <span class="text-[10px] font-medium mt-1 font-manrope">Alerts</span>
        </router-link>

        <router-link
          v-if="isAuthenticated"
          to="/profile"
          class="flex flex-col items-center py-2 px-4 rounded-xl transition-all"
          :class="$route.path === '/profile' ? 'text-white' : 'text-gray-500'"
        >
          <User size="22" :stroke-width="$route.path === '/profile' ? 2.5 : 1.8" />
          <span class="text-[10px] font-medium mt-1 font-manrope">Profile</span>
        </router-link>

        <router-link
          v-else
          to="/login"
          class="flex flex-col items-center py-2 px-4 rounded-xl transition-all"
          :class="$route.path === '/login' ? 'text-white' : 'text-gray-500'"
        >
          <LogIn size="22" :stroke-width="$route.path === '/login' ? 2.5 : 1.8" />
          <span class="text-[10px] font-medium mt-1 font-manrope">Login</span>
        </router-link>
      </div>
    </nav>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import { useToast } from "vue-toastification";
import { Home, Search, Bell, User, LogIn, ChevronDown, Map } from "lucide-vue-next";
import { useAuthStore } from "@/stores/auth.store";
import alertsService from "@/services/api/alerts.service";

const router = useRouter();
const toast = useToast();
const authStore = useAuthStore();

const dropdownOpen = ref(false);
const unreadAlertCount = ref(0);
let alertInterval = null;

const isAuthenticated = computed(() => authStore.isAuthenticated);
const userName = computed(() => authStore.user?.userName || "");
const userInitials = computed(() => {
  const name = userName.value || "U";
  return name.charAt(0).toUpperCase();
});

const toggleDropdown = () => {
  dropdownOpen.value = !dropdownOpen.value;
};

const handleLogout = () => {
  authStore.logout();
  toast.success("Logged out successfully");
  router.push("/login");
  dropdownOpen.value = false;
};

// Load unread alerts count
const loadUnreadAlertCount = async () => {
  if (!isAuthenticated.value) return;

  try {
    const response = await alertsService.getAllAlerts();
    if (response.success && response.data) {
      const unread = response.data.filter((a) => !a.isRead).length;
      unreadAlertCount.value = unread;
    }
  } catch (error) {
    console.error("Failed to load alert count:", error);
  }
};

// Close dropdown when clicking outside
const handleClickOutside = (event) => {
  if (!event.target.closest(".relative")) {
    dropdownOpen.value = false;
  }
};

// Auto-refresh alert count every 60 seconds
const startAlertRefresh = () => {
  if (alertInterval) clearInterval(alertInterval);
  alertInterval = setInterval(() => {
    loadUnreadAlertCount();
  }, 60000);
};

onMounted(() => {
  loadUnreadAlertCount();
  startAlertRefresh();
  document.addEventListener("click", handleClickOutside);
});

onUnmounted(() => {
  if (alertInterval) clearInterval(alertInterval);
  document.removeEventListener("click", handleClickOutside);
});
</script>

<style scoped>
/* Custom scrollbar for desktop */
::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}

::-webkit-scrollbar-track {
  background: #1a2830;
}

::-webkit-scrollbar-thumb {
  background: #3d5a68;
  border-radius: 3px;
}

::-webkit-scrollbar-thumb:hover {
  background: #4a6b7a;
}
</style>
