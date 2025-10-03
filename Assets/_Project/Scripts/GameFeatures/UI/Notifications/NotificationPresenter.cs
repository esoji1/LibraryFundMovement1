using System;
using System.Collections;
using _Project.Core;
using UnityEngine;

namespace _Project.GameFeatures.UI.Notifications
{
    public class NotificationPresenter : IDisposable
    {
        private readonly NotificationPopup _notificationPopup;
        private readonly MonoBehaviour _monoBehaviour;
        private readonly ObjectPoolNotification _objectPoolNotification;

        private Coroutine _coroutine;
        private float _timeBetween = 3f;

        public NotificationPresenter(NotificationPopup notificationPopup, ObjectPoolNotification objectPoolNotification,
            MonoBehaviour monoBehaviour)
        {
            _notificationPopup = notificationPopup;
            _objectPoolNotification = objectPoolNotification;
            _monoBehaviour = monoBehaviour;
            
            _notificationPopup.OnNotificationReceived += OnNotificationReceived;
        }

        public void Dispose() =>
            _notificationPopup.OnNotificationReceived -= OnNotificationReceived;

        private void OnNotificationReceived(string message)
        {
            Stop();
            _notificationPopup.SetNotificationText(message);
            Start();
        }

        private void Start()
        {
            if(_coroutine == null)
                _monoBehaviour.StartCoroutine(WaitingForNotificationRemoved());
        }

        private void Stop()
        {
            if (_coroutine != null)
            {
                _objectPoolNotification.Release(_notificationPopup);
                _monoBehaviour.StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }
        
        private IEnumerator WaitingForNotificationRemoved()
        {
            _notificationPopup.Show();
            yield return new WaitForSeconds(_timeBetween);
            _objectPoolNotification.Release(_notificationPopup);
        }
    }
}