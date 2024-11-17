import * as signalR from "@microsoft/signalr";

class SignalRService {
  private connection: signalR.HubConnection | null = null;

  public async startConnection() {
    if (this.connection) {
      console.log("Connection already exists.");
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5205/taskHub")
      .withAutomaticReconnect()
      .build();

    try {
      await this.connection.start();
      console.log("SignalR connection started");

      this.connection.onreconnected(() => {
        console.log("SignalR connection reestablished");
      });

      this.connection.onreconnecting(() => {
        console.log("SignalR attempting to reconnect");
      });

    } catch (err) {
      console.error("Error starting SignalR connection:", err);
    }
  }

  public async stopConnection() {
    if (this.connection) {
      try {
        await this.connection.stop();
        console.log("SignalR connection stopped");
        this.connection = null;
      } catch (err) {
        console.error("Error stopping SignalR connection:", err);
      }
    }
  }

  public isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }

  public onTaskUpdated(callback: (taskId: number, newStatus: "Pending" | "In Progress" | "Completed") => void) {
    if (this.connection) {
      this.connection.on("TaskUpdated", callback);
    } else {
      console.error("SignalR connection not established. Call startConnection first.");
    }
  }

  public async updateTaskStatus(taskId: number, newStatus: "Pending" | "In Progress" | "Completed") {
    if (this.connection && this.isConnected()) {
      try {
        await this.connection.invoke("UpdateTaskStatus", taskId, newStatus);
      } catch (err) {
        console.error("Error invoking UpdateTaskStatus:", err);
        throw err;
      }
    } else {
      console.error("SignalR connection not established or not active. Call startConnection first.");
      throw new Error("SignalR connection not established or not active");
    }
  }
}

export const signalRService = new SignalRService();

export const updateTaskStatus = async (taskId: number, newStatus: "Pending" | "In Progress" | "Completed") => {
  await signalRService.updateTaskStatus(taskId, newStatus);
};