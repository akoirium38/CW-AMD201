// Import the functions you need from the SDKs you need
import { initializeApp } from "firebase/app";
import { getAnalytics, isSupported, Analytics } from "firebase/analytics";

// Your web app's Firebase configuration
// For Firebase JS SDK v7.20.0 and later, measurementId is optional
const firebaseConfig = {
  apiKey: "AIzaSyBLc2C32bleQCxBsVp6pecAkl3UFevotNQ",
  authDomain: "amd201-cb545.firebaseapp.com",
  projectId: "amd201-cb545",
  storageBucket: "amd201-cb545.firebasestorage.app",
  messagingSenderId: "223016509720",
  appId: "1:223016509720:web:fe9c6fba23b35f39bcbe6d",
  measurementId: "G-11FRPC6KZ9"
};

// Initialize Firebase App
export const app = initializeApp(firebaseConfig);

// Initialize Analytics safely in browser environment
export let analytics: Analytics | null = null;
if (typeof window !== "undefined") {
  isSupported().then((supported) => {
    if (supported) {
      analytics = getAnalytics(app);
      console.log("Firebase Analytics initialized for web dashboard tracking (Measurement ID: G-11FRPC6KZ9)");
    }
  });
}
