<!-- frontend/src/components/LineChip.vue -->
<template>
  <span
    class="inline-flex items-center justify-center rounded-full font-bold"
    :class="sizeClasses"
    :style="{ backgroundColor: lineColor, color: textColor }"
  >
    {{ lineName }}
  </span>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  lineId: {
    type: String,
    required: true,
  },
  size: {
    type: String,
    default: "sm",
    validator: (value) => ["xs", "sm", "md"].includes(value),
  },
});

// Map line IDs to colors (you can expand this or fetch from API)
const lineColors = {
  Red: "#DA291C",
  Orange: "#ED8B00",
  Blue: "#003DA5",
  Green: "#00843D",
  Mattapan: "#DA291C",
  741: "#7C878E",
  742: "#7C878E",
  743: "#7C878E",
  746: "#7C878E",
  "CR-Fairmount": "#80276C",
  "CR-Fitchburg": "#80276C",
  "CR-Worcester": "#80276C",
  "CR-Providence": "#80276C",
};

const lineName = computed(() => {
  // If it's a numeric route, just show the number
  if (/^\d+$/.test(props.lineId)) return props.lineId;
  if (props.lineId.startsWith("CR-")) return props.lineId.replace("CR-", "");
  return props.lineId;
});

const lineColor = computed(() => {
  return lineColors[props.lineId] || "#888888";
});

const textColor = computed(() => {
  // White text for dark colors, black for light
  const darkColors = ["#DA291C", "#ED8B00", "#003DA5", "#00843D", "#80276C", "#7C878E"];
  return darkColors.includes(lineColor.value) ? "#FFFFFF" : "#000000";
});

const sizeClasses = computed(() => {
  switch (props.size) {
    case "xs":
      return "text-[9px] px-1.5 py-0.5 min-w-[32px]";
    case "sm":
      return "text-[10px] px-2 py-0.5 min-w-[36px]";
    case "md":
      return "text-xs px-2.5 py-1 min-w-[42px]";
    default:
      return "text-[10px] px-2 py-0.5 min-w-[36px]";
  }
});
</script>
