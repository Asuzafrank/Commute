<!-- frontend/src/components/TermsModal.vue -->
<template>
  <div v-if="show" class="fixed inset-0 z-50 flex items-center justify-center p-4" style="background: rgba(0,0,0,0.8)">
    <div class="bg-[#354F5C] rounded-xl max-w-2xl w-full max-h-[80vh] flex flex-col border border-[#3D5A68]">
      <!-- Header -->
      <div class="p-4 border-b border-[#3D5A68] flex justify-between items-center">
        <h2 class="text-white text-lg font-bold font-manrope">Terms of Use</h2>
        <button @click="close" class="text-gray-400 hover:text-white">
          <X size="20" />
        </button>
      </div>
      
      <!-- Content - Scrollable -->
      <div class="flex-1 overflow-y-auto p-5 space-y-4 text-gray-300 text-sm font-manrope">
        <div>
          <p class="text-gray-400 text-xs mb-1">Last Updated: April 28, 2026</p>
          <p class="text-gray-400 text-xs mb-4">Effective Date: April 28, 2026</p>
        </div>

        <div>
          <h3 class="text-white font-semibold mb-2">1. Acceptance of Terms</h3>
          <p>By downloading, installing, or using Commute Pro, you agree to be bound by these Terms of Use. If you do not agree, do not use the app.</p>
        </div>

        <div>
          <h3 class="text-white font-semibold mb-2">2. Description of Service</h3>
          <p>Commute Pro is a free mobile application that provides real-time train tracking, weather notifications, and train line occupancy/status updates. We use data from public transit agencies and third-party weather providers. All information is provided "as-is" and may not reflect real-time conditions.</p>
        </div>

        <div>
          <h3 class="text-white font-semibold mb-2">3. User Accounts</h3>
          <p>You may need to create an account to access some features. Keep your login details secure. You are responsible for all activity that occurs under your account.</p>
        </div>

        <div>
          <h3 class="text-white font-semibold mb-2">4. Acceptable Use</h3>
          <p>You agree not to:</p>
          <ul class="list-disc list-inside ml-4 space-y-1 mt-1">
            <li>Use the app for any illegal purpose or to track individuals without their consent</li>
            <li>Interfere with, disrupt, or attempt to gain unauthorized access to our servers or data sources</li>
            <li>Rely solely on Commute Pro for emergency or safety-critical travel decisions</li>
            <li>Copy, modify, or reverse engineer any part of the app</li>
          </ul>
        </div>

        <div>
          <h3 class="text-white font-semibold mb-2">5. Location Data & Notifications</h3>
          <p>Core features require access to your device location to show nearby trains and send relevant alerts. You can manage location permissions in your device settings. Disabling location will limit functionality. Notifications depend on third-party data and network availability. Delays or inaccuracies may occur.</p>
        </div>

        <div>
          <h3 class="text-white font-semibold mb-2">6. Third-Party Data & No Warranty</h3>
          <p>Train schedules, weather, and line status are provided by external transit agencies and weather services. We do not guarantee the accuracy, completeness, timeliness, or availability of any data. Always verify with official transit sources before travel. Commute Pro is provided "as is" and "as available" without warranties of any kind.</p>
        </div>

        <div>
          <h3 class="text-white font-semibold mb-2">7. Limitation of Liability</h3>
          <p>To the maximum extent permitted by Kenyan law, Commute Pro and its creators will not be liable for any indirect, incidental, special, or consequential damages resulting from: missed connections, inaccurate ETAs, service disruptions, weather events, or your use of or inability to use the app. Our total liability for any claim will not exceed 1,000 KES.</p>
        </div>

        <div>
          <h3 class="text-white font-semibold mb-2">8. Privacy</h3>
          <p>Your privacy matters. Our collection and use of personal data, including location, is explained in our Privacy Policy. By using Commute Pro, you also agree to our Privacy Policy. We do not sell personal data.</p>
        </div>

        <div>
          <h3 class="text-white font-semibold mb-2">9. Changes to Service & Terms</h3>
          <p>We may modify the app or these Terms at any time. If we make material changes, we'll notify you through the app. Continued use after changes means you accept the new Terms.</p>
        </div>

        <div>
          <h3 class="text-white font-semibold mb-2">10. Termination</h3>
          <p>We reserve the right to suspend or terminate your access if you violate these Terms. You may stop using Commute Pro at any time by deleting the app from your device.</p>
        </div>

        <div>
          <h3 class="text-white font-semibold mb-2">11. Governing Law</h3>
          <p>These Terms are governed by the laws of the US government.</p>
        </div>

        <div>
          <h3 class="text-white font-semibold mb-2">12. Contact Us</h3>
          <p>For questions about these Terms, contact us at: <a href="mailto:commuteproapp@gmail.com" class="text-orange-400">commuteproapp@gmail.com</a></p>
        </div>
      </div>
      
      <!-- Footer with Accept Button -->
      <div class="p-4 border-t border-[#3D5A68] flex justify-end gap-3">
        <button @click="close" class="px-4 py-2 rounded-lg text-gray-400 hover:text-white transition">
          Close
        </button>
        <button v-if="showAccept" @click="accept" class="px-4 py-2 bg-orange-500 text-gray-900 rounded-lg font-semibold hover:bg-orange-600 transition">
          I Agree
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { X } from 'lucide-vue-next';

const props = defineProps({
  show: {
    type: Boolean,
    default: false
  },
  showAccept: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['close', 'accept']);

const close = () => {
  emit('close');
};

const accept = () => {
  emit('accept');
};
</script>