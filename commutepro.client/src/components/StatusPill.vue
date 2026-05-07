<!-- frontend/src/components/StatusPill.vue -->
<template>
  <div class="flex items-center gap-2">
    <div
      class="w-2 h-2 rounded-full"
      :class="statusClass"
      :style="{ animation: status === 'live' ? 'pulse 1.5s ease-in-out infinite' : 'none' }"
    ></div>
    <span class="text-white text-xs font-semibold font-manrope">
      {{ statusText }}
    </span>
    <span
      v-if="updatedSecondsAgo !== undefined && status === 'live'"
      class="text-gray-500 text-[10px] font-manrope"
    >
      ({{ updatedSecondsAgo }}s ago)
    </span>
  </div>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  status: {
    type: String,
    default: "live",
    validator: (value) => ["live", "delayed", "no-service"].includes(value),
  },
  updatedSecondsAgo: {
    type: Number,
    default: undefined,
  },
});

const statusClass = computed(() => {
  switch (props.status) {
    case "live":
      return "bg-green-500";
    case "delayed":
      return "bg-amber-500";
    case "no-service":
      return "bg-red-500";
    default:
      return "bg-gray-500";
  }
});

const statusText = computed(() => {
  switch (props.status) {
    case "live":
      return "Live";
    case "delayed":
      return "Delayed";
    case "no-service":
      return "No Service";
    default:
      return "Unknown";
  }
});
</script>

<style scoped>
@keyframes pulse {
  0%,
  100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}
</style>
