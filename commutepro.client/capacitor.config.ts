import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.commutePro.app',
  appName: 'CommutePro',
    server: {
    url: 'https://commutepro-app.netlify.app',
    cleartext: false
  }
};

export default config;
