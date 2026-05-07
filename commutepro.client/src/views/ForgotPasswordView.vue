<!-- frontend/src/views/ForgotPasswordView.vue -->
<template>
  <div class="min-h-[80vh] flex items-center justify-center">
    <div class="bg-white rounded-lg shadow-md p-8 w-full max-w-md">
      <div class="text-center mb-8">
        <div class="text-4xl mb-2">🔐</div>
        <h1 class="text-2xl font-bold text-gray-900">Reset Password</h1>
        <p class="text-gray-600 mt-1">We'll send you a link to reset your password</p>
      </div>

      <form @submit.prevent="handleReset" class="space-y-5">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
          <input
            v-model="email"
            type="email"
            required
            class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-commute-blue"
            placeholder="you@example.com"
          />
        </div>

        <button
          type="submit"
          :disabled="loading || !email"
          class="w-full bg-commute-blue text-white py-2 rounded-md font-medium hover:bg-blue-700 transition disabled:opacity-50"
        >
          {{ loading ? "Sending..." : "Send Reset Link" }}
        </button>
      </form>

      <p class="text-center text-sm text-gray-600 mt-6">
        Remember your password?
        <router-link to="/login" class="text-commute-blue hover:underline">
          Back to Login
        </router-link>
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { useToast } from "vue-toastification";

const toast = useToast();
const email = ref("");
const loading = ref(false);

const handleReset = async () => {
  loading.value = true;
  // Simulate API call
  setTimeout(() => {
    toast.success(`Password reset link sent to ${email.value}`);
    loading.value = false;
  }, 1500);
};
</script>
