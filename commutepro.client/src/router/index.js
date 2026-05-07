// frontend/src/router/index.js
import { createRouter, createWebHistory } from "vue-router";
import authService from "@/services/api/auth.service";

// Import views (we'll create these next)
import DefaultLayout from "@/layouts/DefaultLayout.vue";
import HomeView from "@/views/HomeView.vue";
import LoginView from "@/views/LoginView.vue";
import RegisterView from "@/views/RegisterView.vue";
import StationDetailsView from "@/views/StationDetailsView.vue";
import SearchView from "@/views/SearchView.vue";
import AlertsView from "@/views/AlertsView.vue";
import AlertDetailsView from "@/views/AlertDetailsView.vue";
import ProfileView from "@/views/ProfileView.vue";
import TripDetailsView from "@/views/TripDetailsView.vue";
import FavouritesView from "@/views/FavouritesView.vue";
import MapView from "@/views/MapView.vue";

// Route guard - requires authentication
const requireAuth = (to, from, next) => {
  if (authService.isAuthenticated()) {
    next();
  } else {
    next("/login");
  }
};

// Route guard - redirect if already logged in
const requireGuest = (to, from, next) => {
  if (authService.isAuthenticated()) {
    next("/");
  } else {
    next();
  }
};

const routes = [
  {
    path: "/",
    component: DefaultLayout,
    children: [
      {
        path: "",
        name: "Home",
        component: HomeView,
        meta: { title: "Home" },
      },
      {
        path: "search",
        name: "Search",
        component: SearchView,
        meta: { title: "Search Stations" },
      },
      {
        path: "station/:stopId",
        name: "StationDetails",
        component: StationDetailsView,
        props: true,
        meta: { title: "Station Details" },
      },
      {
        path: "/forgot-password",
        name: "ForgotPassword",
        component: () => import("@/views/ForgotPasswordView.vue"),
        meta: { title: "Reset Password" },
        beforeEnter: requireGuest,
      },
      {
        path: "alerts",
        name: "Alerts",
        component: AlertsView,
        meta: { title: "Service Alerts" },
      },
      {
        path: "alerts/:alertId",
        name: "AlertDetails",
        component: AlertDetailsView,
        props: true,
        meta: { title: "Alert Details" },
      },
      {
        path: "profile",
        name: "Profile",
        component: ProfileView,
        meta: { title: "My Profile" },
        beforeEnter: requireAuth,
      },
      {
        path: "favourites",
        name: "Favourites",
        component: FavouritesView,
        meta: { title: "My Stations" },
        beforeEnter: requireAuth,
      },
      {
        path: "trip/:tripId",
        name: "TripDetails",
        component: TripDetailsView,
        props: true,
        meta: { title: "Trip Details" },
      },
      {
        path: "map",
        name: "Map",
        component: MapView,
        meta: { title: "Live Map" },
      },
    ],
  },
  {
    path: "/login",
    name: "Login",
    component: LoginView,
    meta: { title: "Login", layout: "auth" },
    beforeEnter: requireGuest,
  },
  {
    path: "/register",
    name: "Register",
    component: RegisterView,
    meta: { title: "Register", layout: "auth" },
    beforeEnter: requireGuest,
  },
  {
    path: "/:pathMatch(.*)*",
    name: "NotFound",
    component: () => import("@/views/NotFoundView.vue"),
    meta: { title: "Page Not Found" },
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition;
    } else {
      return { top: 0 };
    }
  },
});

// Update page title on route change
router.beforeEach((to, from, next) => {
  document.title = `${to.meta.title || "CommutePro"} | CommutePro`;
  next();
});

export default router;
