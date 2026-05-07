<!-- frontend/src/components/SettingsRow.vue -->
<template>
  <button
    v-if="!hasSlot"
    @click="emitClick"
    class="w-full flex items-center gap-4 px-5 py-4 text-left transition-all hover:bg-white/5"
    style="border-bottom: 1px solid rgba(61, 90, 104, 0.5)"
  >
    <div
      class="w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0"
      :style="{ background: '#3A5060' }"
    >
      <component :is="iconComponent" size="16" :color="iconColor" />
    </div>
    <div class="flex-1 min-w-0">
      <span class="text-white text-sm font-medium font-manrope block">{{ label }}</span>
      <span v-if="sublabel" class="text-gray-500 text-[11px] font-manrope block mt-0.5">{{
        sublabel
      }}</span>
    </div>
    <slot></slot>
    <ChevronRight v-if="!noChevron" size="16" color="#6B8A96" class="flex-shrink-0" />
  </button>

  <div
    v-else
    class="w-full flex items-center gap-4 px-5 py-4"
    style="border-bottom: 1px solid rgba(61, 90, 104, 0.5)"
  >
    <div
      class="w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0"
      :style="{ background: '#3A5060' }"
    >
      <component :is="iconComponent" size="16" :color="iconColor" />
    </div>
    <div class="flex-1 min-w-0">
      <span class="text-white text-sm font-medium font-manrope block">{{ label }}</span>
      <span v-if="sublabel" class="text-gray-500 text-[11px] font-manrope block mt-0.5">{{
        sublabel
      }}</span>
    </div>
    <slot></slot>
  </div>
</template>

<script setup>
import { computed, useSlots } from "vue";

import {
  ChevronRight,
  Bell,
  Lock,
  Smartphone,
  Mail,
  Info,
  Shield,
  FileText,
} from "lucide-vue-next";

const props = defineProps({
  icon: {
    type: String,
    required: true,
  },
  iconColor: {
    type: String,
    default: "#94A3B8",
  },
  label: {
    type: String,
    required: true,
  },
  sublabel: {
    type: String,
    default: "",
  },
  noChevron: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(["click"]);

const hasSlot = computed(() => !!useSlots().default);

const iconComponent = computed(() => {
  const icons = {
    Bell: Bell,
    Lock: Lock,
    Smartphone: Smartphone,
    Mail: Mail,
    Info: Info,
    Shield: Shield,
    FileText: FileText,
  };
  return icons[props.icon] || Info;
});

const emitClick = () => {
  emit("click");
};
</script>
