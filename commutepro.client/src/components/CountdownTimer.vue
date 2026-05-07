<!-- frontend/src/components/CountdownTimer.vue -->
<template>
  <div class="flex flex-col items-end">
    <span
      class="font-mono font-bold tabular-nums"
      :class="sizeClasses"
      :style="{ color: timerColor }"
    >
      {{ displayText }}
    </span>
  </div>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  minutes: {
    type: Number,
    default: null,
  },
  size: {
    type: String,
    default: "md",
    validator: (value) => ["sm", "md", "lg"].includes(value),
  },
});

const displayText = computed(() => {
  if (props.minutes === null || props.minutes === undefined) return "—";
  if (props.minutes <= 0) return "Due";
  if (props.minutes === 1) return "1 min";
  if (props.minutes < 60) return `${props.minutes} min`;
  const hours = Math.floor(props.minutes / 60);
  const mins = props.minutes % 60;
  return `${hours}h ${mins}m`;
});

const timerColor = computed(() => {
  if (props.minutes === null) return "#6B8A96";
  if (props.minutes <= 2) return "#22C55E";
  if (props.minutes <= 5) return "#F5A623";
  return "#94A3B8";
});

const sizeClasses = computed(() => {
  switch (props.size) {
    case "sm":
      return "text-sm";
    case "md":
      return "text-base";
    case "lg":
      return "text-2xl";
    default:
      return "text-base";
  }
});
</script>
