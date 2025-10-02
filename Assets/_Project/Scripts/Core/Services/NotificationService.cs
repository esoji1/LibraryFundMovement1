using System;

namespace _Project.Core.Services
{
    public class NotificationService
    {
        public event Action<string> OnNotificationReceived;

        public void Notify(string message) => OnNotificationReceived?.Invoke(message);
    }
}