<!-- frontend/src/components/PasswordStrengthBar.vue -->
<template>
  <div v-if="password" class="mt-2">
    <div class="flex gap-1 mb-1">
      <div
        v-for="i in 4"
        :key="i"
        class="flex-1 h-1 rounded-full transition-all"
        :style="{ background: i <= strength ? strengthColor : 'rgba(255,255,255,0.1)' }"
      ></div>
    </div>
    <span class="text-[10px] font-semibold font-manrope" :style="{ color: strengthColor }">
      {{ strengthLabel }}
    </span>
  </div>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  password: {
    type: String,
    default: "",
  },
});

const strength = computed(() => {
  const pwd = props.password;
  if (!pwd) return 0;
  if (pwd.length < 6) return 1;
  if (pwd.length < 10 || !/[A-Z]/.test(pwd) || !/[0-9]/.test(pwd)) return 2;
  if (pwd.length >= 12 && /[A-Z]/.test(pwd) && /[0-9]/.test(pwd) && /[^A-Za-z0-9]/.test(pwd))
    return 4;
  return 3;
});

const strengthColor = computed(() => {
  const colors = ["", "#E63946", "#F5A623", "#F5A623", "#22C55E"];
  return colors[strength.value];
});

const strengthLabel = computed(() => {
  const labels = ["", "Weak", "Fair", "Good", "Strong"];
  return labels[strength.value];
});
</script>
