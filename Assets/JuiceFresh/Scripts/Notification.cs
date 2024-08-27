using System.Collections;
using UnityEngine;
using Unity.Notifications.Android;

public class Notification : MonoBehaviour
{
    private void Awake()
    {
        // Создаем канал для уведомлений
        AndroidNotificationChannel channel = new AndroidNotificationChannel
        {
            Id = "Push",
            Name = "Push Notifications",
            Description = "Notification of news",
            Importance = Importance.Default,
        };

        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        // Отправляем уведомление через 5 секунд после запуска игры
        SendTestNotification();
    }

    private void SendTestNotification()
    {
        AndroidNotification notification = new AndroidNotification()
        {
            Title = "Butterflies are waiting for you",
            Text = "Discover new parts of the park and improve the old ones, make your park even better!",
            FireTime = System.DateTime.Now.AddSeconds(5),
            SmallIcon = "small_icon",
        };

        AndroidNotificationCenter.SendNotification(notification, "Push");
    }
}
