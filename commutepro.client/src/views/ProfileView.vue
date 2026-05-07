<!-- frontend/src/views/ProfileView.vue -->
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
      <div class="flex items-center gap-3">
        <button
          @click="goBack"
          class="w-9 h-9 rounded-lg flex items-center justify-center cursor-pointer transition-all hover:bg-white/5"
          style="background: rgba(255, 255, 255, 0.05); border: none"
        >
          <ChevronLeft size="20" color="rgba(255,255,255,0.75)" />
        </button>
        <h1 class="text-white text-xl font-bold font-manrope">Profile</h1>
      </div>
    </div>

    <!-- Not Authenticated State -->
    <div
      v-if="!isAuthenticated"
      class="flex flex-col items-center justify-center px-8 py-20 text-center"
    >
      <div
        class="w-20 h-20 rounded-full flex items-center justify-center mb-6"
        style="background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.08)"
      >
        <User size="36" color="rgba(255,255,255,0.4)" />
      </div>
      <h2 class="text-white text-xl font-bold font-manrope mb-2">Sign in to CommutePro</h2>
      <p class="text-gray-400 text-sm font-manrope leading-relaxed mb-8 text-center">
        Save your favourite stations, get personalised alerts, and sync across devices.
      </p>
      <button
        @click="goToLogin"
        class="bg-orange-500 text-gray-900 border-none rounded-xl px-8 py-4 text-sm font-bold cursor-pointer font-manrope"
      >
        Sign In / Sign Up
      </button>
    </div>

    <!-- Authenticated State -->
    <div v-else>
      <!-- Profile Header -->
      <div class="px-5 pt-8 pb-6 text-center" style="border-bottom: 1px solid #3d5a68">
        <div
          class="w-18 h-18 rounded-full flex items-center justify-center mx-auto mb-4"
          style="background: rgba(255, 255, 255, 0.08)"
        >
          <span class="text-white text-2xl font-bold font-manrope">{{ userInitials }}</span>
        </div>
        <h2 class="text-white text-xl font-bold font-manrope mb-1">{{ userName }}</h2>
        <p class="text-gray-400 text-xs font-manrope">{{ userEmail }}</p>

        <!-- Stats -->
        <div class="flex items-center justify-center gap-6 mt-5">
          <div class="text-center">
            <p class="text-orange-500 text-lg font-bold font-mono">{{ favouriteCount }}</p>
            <p class="text-gray-500 text-[11px] font-manrope">Stations</p>
          </div>
          <div class="w-px h-8 bg-gray-700"></div>
          <div class="text-center">
            <p class="text-orange-500 text-lg font-bold font-mono">{{ alertCount }}</p>
            <p class="text-gray-500 text-[11px] font-manrope">Alerts</p>
          </div>
          <div class="w-px h-8 bg-gray-700"></div>
          <div class="text-center">
            <p class="text-green-500 text-lg font-bold font-mono">Live</p>
            <p class="text-gray-500 text-[11px] font-manrope">Status</p>
          </div>
        </div>
      </div>

      <!-- Notifications Section -->
      <SectionHeader title="Notifications" />
      <div
        class="mx-4 rounded-xl overflow-hidden"
        style="background: #354f5c; border: 1px solid #3d5a68"
      >
        <SettingsRow
          icon="Bell"
          iconColor="#F5A623"
          label="Delay Alerts"
          sublabel="Get notified when your trains are delayed"
        >
          <ToggleSwitch v-model="notifications.delay" @change="saveNotifications" />
        </SettingsRow>
        <SettingsRow
          icon="Bell"
          iconColor="#009FE0"
          label="Service Changes"
          sublabel="Engineering works, route changes"
        >
          <ToggleSwitch v-model="notifications.serviceChanges" @change="saveNotifications" />
        </SettingsRow>
      </div>

      <!-- Account Section -->
      <SectionHeader title="Account" />
      <div
        class="mx-4 rounded-xl overflow-hidden"
        style="background: #354f5c; border: 1px solid #3d5a68"
      >
        <SettingsRow icon="Lock" label="Change Password" @click="changePassword" />
        <SettingsRow
          icon="Smartphone"
          label="Linked Devices"
          sublabel="1 device"
          @click="showDevices"
        />
        <SettingsRow
          icon="Mail"
          label="Email Preferences"
          :sublabel="userEmail"
          @click="emailPreferences"
        />
      </div>

      <!-- App Section -->
      <SectionHeader title="App" />
      <div
        class="mx-4 rounded-xl overflow-hidden"
        style="background: #354f5c; border: 1px solid #3d5a68"
      >
        <!-- Appearance Theme -->
        <div class="px-5 py-4" style="border-bottom: 1px solid rgba(61, 90, 104, 0.5)">
          <div class="flex items-center gap-4 mb-3">
            <div
              class="w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0"
              style="background: #3a5060"
            >
              <Monitor size="16" color="#94A3B8" />
            </div>
            <span class="text-white text-sm font-medium font-manrope">Appearance</span>
          </div>
          <div class="flex gap-2 pl-12">
            <button
              v-for="option in themeOptions"
              :key="option.value"
              @click="selectedTheme = option.value"
              class="flex-1 flex items-center justify-center gap-1.5 rounded-lg py-2 px-3 text-[11px] font-semibold transition-all"
              :style="{
                background:
                  selectedTheme === option.value ? 'rgba(255,255,255,0.08)' : 'transparent',
                border: `1px solid ${selectedTheme === option.value ? 'rgba(255,255,255,0.3)' : 'rgba(255,255,255,0.08)'}`,
                color: selectedTheme === option.value ? '#FFFFFF' : '#4A6478',
              }"
            >
              <component
                :is="option.icon"
                size="13"
                :color="selectedTheme === option.value ? '#FFFFFF' : '#4A6478'"
              />
              {{ option.label }}
            </button>
          </div>
        </div>
        <SettingsRow
          icon="Info"
          label="About CommutePro"
          sublabel="Version 1.0.0"
          @click="showAbout"
        />
        <SettingsRow icon="Shield" label="Privacy Policy" @click="showPrivacy" />
        <SettingsRow icon="FileText" label="Terms of Service" @click="showTerms" />
      </div>

      <!-- Sign Out Button -->
      <div class="mx-4 mt-6">
        <button
          @click="handleLogout"
          class="w-full flex items-center justify-center gap-2 rounded-xl py-4 text-sm font-semibold transition-all"
          style="background: transparent; border: 1px solid rgba(230, 57, 70, 0.4); color: #e63946"
          @mouseenter="(e) => (e.currentTarget.style.background = 'rgba(230,57,70,0.08)')"
          @mouseleave="(e) => (e.currentTarget.style.background = 'transparent')"
        >
          <LogOut size="18" color="#E63946" />
          Sign Out
        </button>
      </div>

      <div class="h-8"></div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from "vue";
import { useRouter } from "vue-router";
import { useToast } from "vue-toastification";
import {
  ChevronLeft,
  User,
  Bell,
  Lock,
  Smartphone,
  Mail,
  Monitor,
  Moon,
  Sun,
  Info,
  Shield,
  FileText,
  LogOut,
} from "lucide-vue-next";
import { useAuthStore } from "@/stores/auth.store";
import favouritesService from "@/services/api/favourites.service";
import alertsService from "@/services/api/alerts.service";
import SectionHeader from "@/components/SectionHeader.vue";
import SettingsRow from "@/components/SettingsRow.vue";
import ToggleSwitch from "@/components/ToggleSwitch.vue";

const router = useRouter();
const toast = useToast();
const authStore = useAuthStore();

// State
const favouriteCount = ref(0);
const alertCount = ref(0);
const notifications = ref({
  delay: true,
  serviceChanges: true,
});
const selectedTheme = ref("dark");

// Theme options
const themeOptions = [
  { value: "dark", icon: Moon, label: "Dark" },
  { value: "light", icon: Sun, label: "Light" },
  { value: "system", icon: Monitor, label: "System" },
];

// Computed
const isAuthenticated = computed(() => authStore.isAuthenticated);
const userName = computed(() => authStore.user?.userName || "");
const userEmail = computed(() => authStore.user?.email || "");
const userInitials = computed(() => {
  const name = userName.value || "U";
  return name.charAt(0).toUpperCase();
});

// Load user data
const loadUserData = async () => {
  if (!isAuthenticated.value) return;

  try {
    // Load favourites count
    const favResponse = await favouritesService.getFavourites();
    if (favResponse.success) {
      favouriteCount.value = favResponse.data?.length || 0;
    }

    // Load alerts count
    const alertResponse = await alertsService.getAllAlerts();
    if (alertResponse.success) {
      alertCount.value = alertResponse.data?.length || 0;
    }
  } catch (error) {
    console.error("Failed to load user data:", error);
  }
};

// Load saved preferences
const loadPreferences = () => {
  const saved = localStorage.getItem("commutepro_preferences");
  if (saved) {
    try {
      const prefs = JSON.parse(saved);
      notifications.value.delay = prefs.delay ?? true;
      notifications.value.serviceChanges = prefs.serviceChanges ?? true;
      selectedTheme.value = prefs.theme ?? "dark";
    } catch (e) {
      console.error("Failed to load preferences:", e);
    }
  }
};

// Save notifications
const saveNotifications = () => {
  savePreferences();
  toast.success("Notification preferences saved");
};

// Save all preferences
const savePreferences = () => {
  const prefs = {
    delay: notifications.value.delay,
    serviceChanges: notifications.value.serviceChanges,
    theme: selectedTheme.value,
  };
  localStorage.setItem("commutepro_preferences", JSON.stringify(prefs));

  // Apply theme
  applyTheme(selectedTheme.value);
};

// Apply theme
const applyTheme = (theme) => {
  if (theme === "dark") {
    document.documentElement.classList.add("dark");
  } else if (theme === "light") {
    document.documentElement.classList.remove("dark");
  } else if (theme === "system") {
    const prefersDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
    if (prefersDark) {
      document.documentElement.classList.add("dark");
    } else {
      document.documentElement.classList.remove("dark");
    }
  }
};

// Navigation
const goBack = () => {
  router.back();
};

const goToLogin = () => {
  router.push("/login");
};

const changePassword = () => {
  toast.info("Password change feature coming soon");
};

const showDevices = () => {
  toast.info("Linked devices feature coming soon");
};

const emailPreferences = () => {
  toast.info("Email preferences feature coming soon");
};

const showAbout = () => {
  toast.info("CommutePro v1.0.0 - Real-time train tracking");
};

const showPrivacy = () => {
  window.open("https://example.com/privacy", "_blank");
};

const showTerms = () => {
  window.open("https://example.com/terms", "_blank");
};

const handleLogout = () => {
  authStore.logout();
  toast.success("Logged out successfully");
  router.push("/login");
};

// Watch theme changes
watch(selectedTheme, (newTheme) => {
  savePreferences();
});

onMounted(() => {
  loadUserData();
  loadPreferences();
});
</script>
