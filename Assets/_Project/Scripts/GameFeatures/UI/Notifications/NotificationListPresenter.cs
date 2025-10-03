using System;
using _Project.Core;
using _Project.Core.Services;
using Zenject;

namespace _Project.GameFeatures.UI.Notifications
{
    public class NotificationListPresenter : IInitializable, IDisposable
    {
        private readonly NotificationList _notificationList;
        private readonly NotificationService _notificationService;

        private ObjectPoolNotification _notificationPopupPoolNotification;

        public NotificationListPresenter(NotificationService notificationService, NotificationList notificationList,
            MonoBehaviorComponent monoBehaviorComponent)
        {
            _notificationService = notificationService;
            _notificationList = notificationList;

            _notificationPopupPoolNotification =
                new ObjectPoolNotification(8, _notificationList, monoBehaviorComponent);
        }

        public void Initialize() => _notificationService.OnNotificationReceived += OnNotificationReceived;

        public void Dispose() => _notificationService.OnNotificationReceived -= OnNotificationReceived;

        private void OnNotificationReceived(string message) => _notificationPopupPoolNotification.Get(message);
    }
}