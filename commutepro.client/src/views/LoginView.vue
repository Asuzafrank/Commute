<!-- frontend/src/views/LoginView.vue -->
<template>
  <div class="min-h-screen flex items-center justify-center" style="background: #243540">
    <div class="w-full max-w-md px-6 py-8">
      <!-- Logo -->
      <div class="flex flex-col items-center mb-6">
        <img src="/src/assets/logo.png" alt="CommutePro" class="h-12 w-auto mb-3" />
        <span class="text-gray-500 text-xs font-manrope">Real-time train tracking</span>
      </div>

      <!-- Mode Tabs -->
      <div
        class="flex rounded-xl p-1 mb-6"
        style="background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.08)"
      >
        <button
          v-for="mode in modes"
          :key="mode.value"
          @click="activeMode = mode.value"
          class="flex-1 py-2 rounded-lg text-xs font-bold font-manrope transition-all"
          :class="activeMode === mode.value ? 'bg-white/10 text-white' : 'text-gray-500'"
        >
          {{ mode.label }}
        </button>
      </div>

      <Transition name="fade" mode="out-in">
        <!-- Login Form -->
        <form
          v-if="activeMode === 'login'"
          key="login"
          @submit.prevent="handleLogin"
          class="space-y-4"
        >
          <div>
            <input
              v-model="loginForm.email"
              type="email"
              required
              class="w-full px-4 py-3 rounded-xl text-white text-sm font-manrope outline-none transition-all"
              style="background: #354f5c; border: 1px solid #3d5a68; caret-color: #ffffff"
              placeholder="Email"
            />
          </div>

          <div class="relative">
            <input
              v-model="loginForm.password"
              :type="showLoginPassword ? 'text' : 'password'"
              required
              class="w-full px-4 py-3 rounded-xl text-white text-sm font-manrope outline-none transition-all"
              style="
                background: #354f5c;
                border: 1px solid #3d5a68;
                caret-color: #ffffff;
                padding-right: 48px;
              "
              placeholder="Password"
            />
            <button
              type="button"
              @click="showLoginPassword = !showLoginPassword"
              class="absolute right-3 top-1/2 -translate-y-1/2 bg-transparent border-none cursor-pointer p-1"
            >
              <EyeOff v-if="showLoginPassword" size="18" color="#6B8A96" />
              <Eye v-else size="18" color="#6B8A96" />
            </button>
          </div>

          <div class="flex justify-end">
            <button
              type="button"
              class="bg-transparent border-none text-gray-500 text-xs font-manrope hover:text-gray-400 transition"
            >
              Forgot password?
            </button>
          </div>

          <div
            v-if="loginError"
            class="flex items-center gap-2 p-2 rounded-xl"
            style="background: rgba(230, 57, 70, 0.1); border: 1px solid rgba(230, 57, 70, 0.3)"
          >
            <AlertCircle size="14" color="#E63946" />
            <span class="text-red-400 text-xs font-manrope">{{ loginError }}</span>
          </div>

          <button
            type="submit"
            :disabled="loading"
            class="w-full py-3 rounded-xl text-sm font-bold cursor-pointer transition-all font-manrope"
            style="background: #ff8c00; color: #243540"
            :class="loading ? 'opacity-50 cursor-not-allowed' : 'hover:bg-orange-600'"
          >
            {{ loading ? "Signing in..." : "Sign In" }}
          </button>

          <!-- Demo Credentials Hint -->
          <div class="text-center pt-2">
            <p class="text-gray-600 text-[10px] font-manrope">
              Demo: githubrian331@gmail.com / Password123!
            </p>
          </div>
        </form>

        <!-- Register Form -->
        <form v-else key="register" @submit.prevent="handleRegister" class="space-y-3">
         

          <div>
            <input
              v-model="registerForm.email"
              type="email"
              required
              class="w-full px-4 py-3 rounded-xl text-white text-sm font-manrope outline-none transition-all"
              :style="{
                background: '#354F5C',
                border: `1px solid ${registerErrors.email ? '#E63946' : '#3D5A68'}`,
                caretColor: '#FFFFFF',
              }"
              placeholder="Email"
            />
            <p v-if="registerErrors.email" class="text-red-400 text-[10px] font-manrope mt-1">
              {{ registerErrors.email }}
            </p>
          </div>

          <div class="relative">
            <input
              v-model="registerForm.password"
              :type="showRegisterPassword ? 'text' : 'password'"
              required
              class="w-full px-4 py-3 rounded-xl text-white text-sm font-manrope outline-none transition-all"
              :style="{
                background: '#354F5C',
                border: `1px solid ${registerErrors.password ? '#E63946' : '#3D5A68'}`,
                caretColor: '#FFFFFF',
                paddingRight: '48px',
              }"
              placeholder="Password (min 6 characters)"
            />
            <button
              type="button"
              @click="showRegisterPassword = !showRegisterPassword"
              class="absolute right-3 top-1/2 -translate-y-1/2 bg-transparent border-none cursor-pointer p-1"
            >
              <EyeOff v-if="showRegisterPassword" size="18" color="#6B8A96" />
              <Eye v-else size="18" color="#6B8A96" />
            </button>
          </div>
          <PasswordStrengthBar :password="registerForm.password" />
          <p v-if="registerErrors.password" class="text-red-400 text-[10px] font-manrope">
            {{ registerErrors.password }}
          </p>

          <div class="relative">
            <input
              v-model="registerForm.userName"
              type="text"
              required
              @input="checkUsernameAvailability"
              class="w-full px-4 py-3 rounded-xl text-white text-sm font-manrope outline-none transition-all"
              :style="{
                background: '#354F5C',
                border: `1px solid ${getUsernameBorderColor()}`,
                caretColor: '#FFFFFF',
                paddingRight: '48px',
              }"
              placeholder="Username"
            />
            <div class="absolute right-3 top-1/2 -translate-y-1/2">
              <div
                v-if="usernameStatus === 'checking'"
                class="w-4 h-4 rounded-full border-2 border-orange-500 border-t-transparent animate-spin"
              ></div>
              <Check v-else-if="usernameStatus === 'available'" size="16" color="#22C55E" />
              <X v-else-if="usernameStatus === 'taken'" size="16" color="#E63946" />
            </div>
          </div>
          <p v-if="usernameStatus === 'available'" class="text-green-500 text-[10px] font-manrope">
            Username is available
          </p>
          <p v-if="registerErrors.userName" class="text-red-400 text-[10px] font-manrope">
            {{ registerErrors.userName }}
          </p>

          <button
            type="button"
            @click="agreeTerms = !agreeTerms"
            class="flex items-start gap-2 w-full text-left bg-transparent border-none cursor-pointer p-0"
          >
            <div
              class="w-4 h-4 rounded flex items-center justify-center flex-shrink-0 mt-0.5"
              :style="{
                border: `2px solid ${agreeTerms ? 'rgba(255,255,255,0.6)' : registerErrors.terms ? '#E63946' : 'rgba(255,255,255,0.15)'}`,
                background: agreeTerms ? 'rgba(255,255,255,0.15)' : 'transparent',
              }"
            >
              <Check v-if="agreeTerms" size="10" color="#FFFFFF" stroke-width="3" />
            </div>
            <span class="text-gray-500 text-[11px] font-manrope leading-relaxed">
              I agree to the <span class="text-gray-300">Terms</span> and
              <span class="text-gray-300">Privacy</span>
            </span>
          </button>
          <p v-if="registerErrors.terms" class="text-red-400 text-[10px] font-manrope">
            {{ registerErrors.terms }}
          </p>

          <button
            type="submit"
            :disabled="loading"
            class="w-full py-3 rounded-xl text-sm font-bold cursor-pointer transition-all font-manrope mt-2"
            style="background: #ff8c00; color: #243540"
            :class="loading ? 'opacity-50 cursor-not-allowed' : 'hover:bg-orange-600'"
          >
            {{ loading ? "Creating account..." : "Create Account" }}
          </button>
        </form>
      </Transition>
      <TermsModal 
  :show="showTermsModal" 
  :showAccept="true"
  @close="showTermsModal = false"
  @accept="acceptTerms"
/>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import { useToast } from "vue-toastification";
import { Eye, EyeOff, AlertCircle, Check, X } from "lucide-vue-next";
import TermsModal from '@/components/TermsModal.vue';
import { useAuthStore } from "@/stores/auth.store";
import PasswordStrengthBar from "@/components/PasswordStrengthBar.vue";
import GoogleIcon from "@/components/icons/GoogleIcon.vue";
import AppleIcon from "@/components/icons/AppleIcon.vue";

const router = useRouter();
const toast = useToast();
const authStore = useAuthStore();

const activeMode = ref("login");
const modes = [
  { value: "login", label: "Sign In" },
  { value: "signup", label: "Sign Up" },
];
const showTermsModal = ref(false);
const hasAcceptedTerms = ref(false);

// Login
const loginForm = ref({ email: "", password: "" });
const showLoginPassword = ref(false);
const loginError = ref("");
const loading = ref(false);

// Register
const registerForm = ref({ email: "", password: "", userName: "" });
const showRegisterPassword = ref(false);
const registerErrors = ref({});
const agreeTerms = ref(false);
const usernameStatus = ref("idle");

const handleLogin = async () => {
  loginError.value = "";
  loading.value = true;
  try {
    const result = await authStore.login({
      email: loginForm.value.email,
      password: loginForm.value.password,
    });
    if (result.success) {
      toast.success(`Welcome back, ${result.data.userName || loginForm.value.email}!`);
      router.push("/");
    } else {
      loginError.value = result.message || "Login failed";
    }
  } catch (error) {
    loginError.value = "Unable to login. Please try again.";
  } finally {
    loading.value = false;
  }
};

const handleRegister = async () => {
  registerErrors.value = {};
  
  if (!registerForm.value.email || !/\S+@\S+\.\S+/.test(registerForm.value.email)) {
    registerErrors.value.email = "Valid email required";
  }
  if (!registerForm.value.password || registerForm.value.password.length < 6) {
    registerErrors.value.password = "Password must be at least 6 characters";
  }
  if (usernameStatus.value !== "available") {
    registerErrors.value.userName = "Choose an available username";
  }
  
if (registerForm.value.userName.length < 3) {
  registerErrors.value.userName = "Username must be at least 3 characters";
}
  if (!agreeTerms.value) registerErrors.value.terms = "Accept terms to continue";

  if (Object.keys(registerErrors.value).length > 0) return;

  loading.value = true;
  try {
    const result = await authStore.register({
      userName: registerForm.value.userName,
      email: registerForm.value.email,
      password: registerForm.value.password,
    });
    if (result.success) {
      toast.success("Account created! Please login.");
      activeMode.value = "login";
      registerForm.value = {email: "", password: "", userName: "" };
    } else {
      toast.error(result.message || "Registration failed");
    }
  } catch (error) {
    toast.error("Unable to create account");
  } finally {
    loading.value = false;
  }
};

// Check if user has accepted terms
const checkTermsAcceptance = () => {
  const accepted = localStorage.getItem('commutepro_terms_accepted');
  if (!accepted) {
    showTermsModal.value = true;
  }
};

const acceptTerms = () => {
  localStorage.setItem('commutepro_terms_accepted', new Date().toISOString());
  hasAcceptedTerms.value = true;
  showTermsModal.value = false;
};
const checkUsernameAvailability = () => {
  const username = registerForm.value.userName;
  if (username.length < 3) {
    usernameStatus.value = "idle";
    return;
  }
  usernameStatus.value = "checking";
  setTimeout(() => {
    const taken = ["admin", "user", "test", "commuter"];
    if (taken.includes(username.toLowerCase())) {
      usernameStatus.value = "taken";
      registerErrors.value.userName = "Username taken";
    } else {
      usernameStatus.value = "available";
      delete registerErrors.value.userName;
    }
  }, 300);
};

const getUsernameBorderColor = () => {
  if (usernameStatus.value === "available") return "#22C55E";
  if (usernameStatus.value === "taken") return "#E63946";
  if (registerErrors.value.userName) return "#E63946";
  return "#3D5A68";
};

onMounted(() => {
  checkTermsAcceptance();
});
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition:
    opacity 0.15s ease,
    transform 0.15s ease;
}
.fade-enter-from {
  opacity: 0;
  transform: translateX(8px);
}
.fade-leave-to {
  opacity: 0;
  transform: translateX(-8px);
}
</style>
