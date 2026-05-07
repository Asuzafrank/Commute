<!-- frontend/src/views/RegisterView.vue -->
<template>
  <div class="min-h-[80vh] flex items-center justify-center">
    <div class="bg-white rounded-lg shadow-md p-8 w-full max-w-md">
      <div class="text-center mb-8">
        <div class="text-4xl mb-2">🚆</div>
        <h1 class="text-2xl font-bold text-gray-900">Create Account</h1>
        <p class="text-gray-600 mt-1">Join CommutePro for personalized updates</p>
      </div>

      <form @submit.prevent="handleRegister" class="space-y-5">
        <!-- User Name -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">User Name</label>
          <input
            v-model="userName"
            type="text"
            required
            class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-commute-blue"
            placeholder="johndoe"
          />
        </div>

        <!-- Email -->
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

        <!-- Password -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Password</label>
          <input
            v-model="password"
            type="password"
            required
            class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-commute-blue"
            placeholder="••••••••"
          />
          <p class="text-xs text-gray-500 mt-1">Must be at least 6 characters</p>
        </div>

        <!-- Confirm Password -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Confirm Password</label>
          <input
            v-model="confirmPassword"
            type="password"
            required
            class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-commute-blue"
            placeholder="••••••••"
          />
        </div>

        <!-- Password Match Error -->
        <p v-if="passwordMismatch" class="text-red-500 text-sm">Passwords do not match</p>

        <!-- Terms -->
        <div class="flex items-center gap-2">
          <input
            type="checkbox"
            v-model="agreeTerms"
            id="terms"
            class="w-4 h-4 text-commute-blue focus:ring-commute-blue"
          />
          <label for="terms" class="text-sm text-gray-600">
            I agree to the
            <a href="#" class="text-commute-blue hover:underline">Terms of Service</a>
            and
            <a href="#" class="text-commute-blue hover:underline">Privacy Policy</a>
          </label>
        </div>

        <!-- Submit Button -->
        <button
          type="submit"
          :disabled="loading || !isFormValid"
          class="w-full bg-commute-blue text-white py-2 rounded-md font-medium hover:bg-blue-700 transition disabled:opacity-50"
        >
          {{ loading ? "Creating account..." : "Sign Up" }}
        </button>
      </form>

      <p class="text-center text-sm text-gray-600 mt-6">
        Already have an account?
        <router-link to="/login" class="text-commute-blue hover:underline"> Sign in </router-link>
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import { useRouter } from "vue-router";
import { useToast } from "vue-toastification";
import { useAuthStore } from "@/stores/auth.store";

const router = useRouter();
const toast = useToast();
const authStore = useAuthStore();

// Form fields
const userName = ref("");
const email = ref("");
const password = ref("");
const confirmPassword = ref("");
const agreeTerms = ref(false);
const loading = ref(false);

// Validation
const passwordMismatch = computed(() => {
  return password.value !== confirmPassword.value && confirmPassword.value !== "";
});

const isFormValid = computed(() => {
  return (
    userName.value.length >= 3 &&
    email.value.includes("@") &&
    password.value.length >= 6 &&
    !passwordMismatch.value &&
    agreeTerms.value
  );
});

// Register handler
const handleRegister = async () => {
  if (!isFormValid.value) return;

  loading.value = true;

  try {
    const result = await authStore.register({
      userName: userName.value,
      email: email.value,
      password: password.value,
    });

    if (result.success) {
      toast.success("Account created successfully! Please login.");
      router.push("/login");
    } else {
      toast.error(result.message || "Registration failed");
    }
  } catch (error) {
    console.error("Registration error:", error);
    const message = error.response?.data?.message || "Unable to create account. Please try again.";
    toast.error(message);
  } finally {
    loading.value = false;
  }
};
</script>
