using System.Collections.Generic;
using System.Linq;
using _Project.GameFeatures.UI.Common;
using _Project.GameFeatures.UI.Notifications;

namespace _Project.Core
{
    public class ObjectPoolNotification
    {
        private readonly ListView<NotificationPopup> _listView;
        private readonly MonoBehaviorComponent _monoBehaviorComponent;

        private List<NotificationPopup> _objects = new();

        public ObjectPoolNotification(int prewarnObjects, ListView<NotificationPopup> listView,
            MonoBehaviorComponent monoBehaviorComponent)
        {
            _listView = listView;
            _monoBehaviorComponent = monoBehaviorComponent;

            for (int i = 0; i < prewarnObjects; i++)
            {
                NotificationPopup obj = Create();
                obj.gameObject.SetActive(false);
                _objects.Add(obj);
            }
        }

        public NotificationPopup Get(string message)
        {
            NotificationPopup notificationPopup = _objects.FirstOrDefault(x => x.isActiveAndEnabled == false);

            if (notificationPopup == null)
                notificationPopup = Create();

            ShowAndNotify(notificationPopup, message);
            return notificationPopup;
        }

        public void Release(NotificationPopup notificationPopup) => notificationPopup.gameObject.SetActive(false);

        private NotificationPopup Create()
        {
            NotificationPopup notificationPopup = CreateNotificationElements();
            _objects.Add(notificationPopup);
            return notificationPopup;
        }

        private void ShowAndNotify(NotificationPopup notificationPopup, string message)
        {
            notificationPopup.gameObject.SetActive(true);
            notificationPopup.Notify(message);
        }

        private NotificationPopup CreateNotificationElements()
        {
            NotificationPopup notificationPopup = _listView.SpawnElement();
            NotificationPresenter notificationPresenter =
                new NotificationPresenter(notificationPopup, this, _monoBehaviorComponent);

            return notificationPopup;
        }
    }
}