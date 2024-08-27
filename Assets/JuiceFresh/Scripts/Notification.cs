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

        // Запускаем отправку уведомлений через заданные интервалы времени
        ScheduleDailyNotifications();
    }

    private void ScheduleDailyNotifications()
    {
        // Удаляем все предыдущие уведомления, чтобы избежать дублирования
        AndroidNotificationCenter.CancelAllScheduledNotifications();

        // Устанавливаем уведомление на первую половину дня
        SendNotificationAtTime(10, 0); // 10:00 AM

        // Устанавливаем уведомление на вторую половину дня
        SendNotificationAtTime(18, 0); // 6:00 PM
    }

    private void SendNotificationAtTime(int hour, int minute)
    {
        System.DateTime now = System.DateTime.Now;
        System.DateTime fireTime = new System.DateTime(now.Year, now.Month, now.Day, hour, minute, 0);

        // Если текущее время уже прошло, устанавливаем уведомление на следующий день
        if (fireTime < now)
        {
            fireTime = fireTime.AddDays(1);
        }

        AndroidNotification notification = new AndroidNotification()
        {
            Title = "Butterflies are waiting for you",
            Text = "Discover new parts of the park and improve the old ones, make your park even better!",
            FireTime = fireTime,
            SmallIcon = "small_icon",
        };

        AndroidNotificationCenter.SendNotification(notification, "Push");
    }
}
