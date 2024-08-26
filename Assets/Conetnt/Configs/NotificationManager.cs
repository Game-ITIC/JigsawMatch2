using UnityEngine;
using Unity.Notifications.Android;
using System;
public class NotificationManager : MonoBehaviour
{
    [SerializeField] private NotificationConfig notificationConfig;

    void Start()
    {
        CreateNotificationChannel();
    }

    void OnApplicationQuit()
    {
        ScheduleNotifications();
    }

    void CreateNotificationChannel()
    {
        var channel = new AndroidNotificationChannel()
        {
            Id = "important_channel",
            Name = "Important Notifications",
            Importance = Importance.High, // Высокий приоритет для звука и вибрации
            Description = "Notifications with sound and vibration",
            EnableVibration = true, // Включаем вибрацию
            LockScreenVisibility = LockScreenVisibility.Public,
            EnableLights = true,
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    void ScheduleNotifications()
    {
        var notifications = notificationConfig.ToAndroidNotifications();
        foreach (var notification in notifications)
        {
            AndroidNotificationCenter.SendNotification(notification, notificationConfig.ChannelId);
        }
    }
}