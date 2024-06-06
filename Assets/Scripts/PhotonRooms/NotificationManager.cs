using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace DefaultNamespace.PhotonRooms
{
    public class NotificationManager : MonoBehaviour
    {
        public GameObject notificationPrefab; // Prefab with Text or TextMeshPro
        public Transform notificationParent; // Parent object to hold notifications
        public float displayDuration = 3.0f; // Time in seconds to display each notification

        private Queue<GameObject> notifications = new Queue<GameObject>();

        void Start()
        {
            // Optionally clear any existing notifications
            foreach (Transform child in notificationParent)
            {
                Destroy(child.gameObject);
            }
        }

        public void ShowNotification(string message)
        {
            GameObject newNotification = Instantiate(notificationPrefab, notificationParent);
            TextMeshProUGUI notificationText = newNotification.GetComponent<TextMeshProUGUI>(); // or TextMeshPro
            notificationText.text = message;

            notifications.Enqueue(newNotification);
            StartCoroutine(RemoveNotificationAfterTime(newNotification, displayDuration));
        }

        private IEnumerator RemoveNotificationAfterTime(GameObject notification, float delay)
        {
            yield return new WaitForSeconds(delay);
            notifications.Dequeue();
            Destroy(notification);
        }
    }
}
