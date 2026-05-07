<!-- frontend/src/components/SectionHeader.vue -->
<template>
  <div class="px-5 pt-5 pb-2">
    <span class="text-gray-500 text-[11px] font-bold font-manrope tracking-wide uppercase">
      {{ title }}
    </span>
  </div>
</template>

<script setup>
defineProps({
  title: {
    type: String,
    required: true,
  },
});
</script>
