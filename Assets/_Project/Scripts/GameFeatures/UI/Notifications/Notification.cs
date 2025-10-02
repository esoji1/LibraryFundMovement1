using TMPro;
using UnityEngine;

namespace _Project.GameFeatures.UI.Notifications
{
    public class NotificationView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _notificationText;

        public void SetNotificationText(string text) => _notificationText.text = text;
    }
}