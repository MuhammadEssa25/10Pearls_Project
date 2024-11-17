using Microsoft.AspNetCore.SignalR;

namespace task_management.Services
{
    public class TaskHub : Hub
    {
        public async Task UpdateTaskStatus(int taskId, string newStatus)
        {
            await Clients.All.SendAsync("TaskUpdated", taskId, newStatus);
        }
    }
}