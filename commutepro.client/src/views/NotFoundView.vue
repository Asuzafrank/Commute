<!-- frontend/src/views/NotFoundView.vue -->
<template>
  <div class="min-h-screen flex items-center justify-center px-4" style="background: #243540">
    <div class="text-center max-w-md">
      <!-- Animated Train Illustration -->
      <div class="mb-8">
        <div class="text-8xl mb-4 animate-bounce">🚆</div>
        <div
          class="relative h-1 rounded-full overflow-hidden"
          style="background: rgba(255, 255, 255, 0.06)"
        >
          <div
            class="absolute top-0 left-0 h-full w-1/3 rounded-full animate-pulse"
            style="background: #ff8c00"
          ></div>
        </div>
      </div>

      <!-- Error Code -->
      <h1 class="text-8xl font-bold mb-2" style="color: #ff8c00">404</h1>
      <h2 class="text-xl font-semibold mb-4" style="color: #94a3b8">Page Not Found</h2>

      <!-- Message -->
      <p class="text-gray-400 text-sm font-manrope mb-8 leading-relaxed">
        Oops! The page you're looking for seems to have left the station.<br />
        It might have been removed, renamed, or never existed.
      </p>

      <!-- Action Buttons -->
      <div class="flex flex-col sm:flex-row gap-4 justify-center">
        <button
          @click="goBack"
          class="flex items-center justify-center gap-2 px-6 py-3 rounded-xl text-sm font-semibold transition-all cursor-pointer"
          style="
            background: rgba(255, 255, 255, 0.05);
            border: 1px solid rgba(255, 255, 255, 0.1);
            color: #94a3b8;
          "
          @mouseenter="(e) => (e.currentTarget.style.background = 'rgba(255,255,255,0.1)')"
          @mouseleave="(e) => (e.currentTarget.style.background = 'rgba(255,255,255,0.05)')"
        >
          <ChevronLeft size="18" />
          Go Back
        </button>

        <router-link
          to="/"
          class="flex items-center justify-center gap-2 px-6 py-3 rounded-xl text-sm font-semibold transition-all no-underline"
          style="background: #ff8c00; color: #243540"
          @mouseenter="(e) => (e.currentTarget.style.background = '#E67E00')"
          @mouseleave="(e) => (e.currentTarget.style.background = '#FF8C00')"
        >
          <Home size="18" />
          Go Home
        </router-link>
      </div>

      <!-- Quick Links -->
      <div class="mt-8 pt-6" style="border-top: 1px solid rgba(255, 255, 255, 0.05)">
        <p class="text-gray-500 text-xs font-manrope mb-3">Try these popular stations:</p>
        <div class="flex flex-wrap gap-2 justify-center">
          <router-link
            to="/station/74614"
            class="text-sm text-gray-400 hover:text-white transition-colors no-underline"
          >
            Silver Line Way
          </router-link>
          <span class="text-gray-600">•</span>
          <router-link
            to="/station/place-north"
            class="text-sm text-gray-400 hover:text-white transition-colors no-underline"
          >
            North Station
          </router-link>
          <span class="text-gray-600">•</span>
          <router-link
            to="/station/place-south"
            class="text-sm text-gray-400 hover:text-white transition-colors no-underline"
          >
            South Station
          </router-link>
          <span class="text-gray-600">•</span>
          <router-link
            to="/station/place-harvd"
            class="text-sm text-gray-400 hover:text-white transition-colors no-underline"
          >
            Harvard Square
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useRouter } from "vue-router";
import { ChevronLeft, Home } from "lucide-vue-next";

const router = useRouter();

const goBack = () => {
  router.go(-1);
};
</script>

<style scoped>
@keyframes bounce {
  0%,
  100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-10px);
  }
}

.animate-bounce {
  animation: bounce 1s ease-in-out infinite;
}

@keyframes pulse {
  0%,
  100% {
    opacity: 0.6;
  }
  50% {
    opacity: 1;
  }
}

.animate-pulse {
  animation: pulse 1.5s ease-in-out infinite;
}
</style>
