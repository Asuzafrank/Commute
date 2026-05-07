// frontend/src/services/signalr.service.js
import * as signalR from "@microsoft/signalr";

class SignalRService {
  constructor() {
    this.connection = null;
    this.stationId = null;
    this.isConnected = false;
    this.reconnectAttempts = 0;
    this.maxReconnectAttempts = 5;

    // Callbacks
    this.onArrivalsUpdate = null;
    this.onDelayAlert = null;
    this.onDataStale = null;
    this.onConnected = null;
    this.onDisconnected = null;
  }

  /**
   * Connect to SignalR hub and subscribe to station
   * @param {string} stationId - Station ID to subscribe to
   * @param {string} token - JWT token for authentication
   */
  async connect(stationId, token) {
    if (this.connection && this.stationId === stationId && this.isConnected) {
      console.log("Already connected to station:", stationId);
      return;
    }

    // Disconnect existing connection
    await this.disconnect();

    this.stationId = stationId;

    // Get base URL from environment
    const baseUrl = import.meta.env.VITE_API_BASE_URL || "https://localhost:7213/api";
    // Remove /api from end for SignalR hub
    const hubUrl = baseUrl.replace("/api", "/realtimeHub");

    // Get auth token
    const authToken = token || localStorage.getItem("accessToken");

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => {
          const currentToken = localStorage.getItem("accessToken");
          return currentToken || "";
        },
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Set up event handlers
    this.setupEventHandlers();

    try {
      await this.connection.start();
      this.isConnected = true;
      this.reconnectAttempts = 0;
      console.log("SignalR connected successfully");

      // Subscribe to station
      await this.connection.invoke("SubscribeToStation", stationId);
      console.log("Subscribed to station:", stationId);

      if (this.onConnected) {
        this.onConnected();
      }
    } catch (error) {
      console.error("SignalR connection failed:", error);
      this.isConnected = false;

      if (this.onDisconnected) {
        this.onDisconnected(error);
      }
    }
  }

  /**
   * Set up SignalR event handlers
   */
  setupEventHandlers() {
    if (!this.connection) return;

    // Handle arrival updates
    this.connection.on("ArrivalsUpdated", (data) => {
      console.log("Arrivals updated via SignalR:", data);
      if (this.onArrivalsUpdate) {
        this.onArrivalsUpdate(data);
      }
    });

    // Handle delay alerts
    this.connection.on("DelayAlert", (alert) => {
      console.log("Delay alert received:", alert);
      if (this.onDelayAlert) {
        this.onDelayAlert(alert);
      }
    });

    // Handle stale data warning
    this.connection.on("DataStale", (data) => {
      console.warn("Data stale warning:", data);
      if (this.onDataStale) {
        this.onDataStale(data);
      }
    });

    // Handle reconnection
    this.connection.onreconnecting((error) => {
      console.log("SignalR reconnecting:", error);
    });

    this.connection.onreconnected((connectionId) => {
      console.log("SignalR reconnected:", connectionId);
      this.isConnected = true;
      // Re-subscribe to station
      if (this.stationId) {
        this.connection.invoke("SubscribeToStation", this.stationId);
      }
      if (this.onConnected) {
        this.onConnected();
      }
    });

    this.connection.onclose((error) => {
      console.log("SignalR connection closed:", error);
      this.isConnected = false;
      if (this.onDisconnected) {
        this.onDisconnected(error);
      }
    });
  }

  /**
   * Disconnect from SignalR
   */
  async disconnect() {
    if (this.connection) {
      if (this.stationId && this.isConnected) {
        try {
          await this.connection.invoke("UnsubscribeFromStation", this.stationId);
          console.log("Unsubscribed from station:", this.stationId);
        } catch (error) {
          console.error("Error unsubscribing:", error);
        }
      }

      try {
        await this.connection.stop();
        console.log("SignalR disconnected");
      } catch (error) {
        console.error("Error stopping connection:", error);
      }

      this.connection = null;
      this.isConnected = false;
      this.stationId = null;
    }
  }

  /**
   * Get connection status
   */
  getConnectionStatus() {
    return {
      isConnected: this.isConnected,
      stationId: this.stationId,
    };
  }
}

// Export singleton instance
export const signalRService = new SignalRService();
