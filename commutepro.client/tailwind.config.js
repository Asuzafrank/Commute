/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{vue,js,ts,jsx,tsx}"],
  theme: {
    extend: {
      colors: {
        "commute-blue": "#1a73e8",
        "delay-amber": "#f59e0b",
        "delay-red": "#dc2626",
        "delay-green": "#10b981",
      },
      fontFamily: {
        manrope: ["Manrope", "sans-serif"],
        mono: ["DM Mono", "monospace"],
      },
    },
  },
  plugins: [],
};
