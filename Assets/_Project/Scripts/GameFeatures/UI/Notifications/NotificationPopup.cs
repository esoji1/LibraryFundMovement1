using System;
using TMPro;
using UnityEngine;

namespace _Project.GameFeatures.UI.Notifications
{
    public class NotificationPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _notificationText;

        public event Action<string> OnNotificationReceived;
        
        public void SetNotificationText(string text) => _notificationText.text = text;
        
        public void Notify(string message) =>  OnNotificationReceived?.Invoke(message);

        public void Show() => gameObject.SetActive(true);
    }
}