import "./assets/main.css";
import Toast from "vue-toastification";
import { createPinia } from "pinia";
import "vue-toastification/dist/index.css";
import { createApp } from "vue";
import router from "./router";
import App from "./App.vue";

const pinia = createPinia();
const app = createApp(App);
app.use(pinia);
app.use(router);
app.use(Toast);
app.mount("#app");
