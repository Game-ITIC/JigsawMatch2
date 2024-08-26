
using System.Collections.Generic;
using System;
using Unity.Notifications.Android;
using UnityEngine;

[CreateAssetMenu(menuName = "Push Notification Config")]
public class NotificationConfig : ScriptableObject
{
    [Serializable]
    public class NotificationContent
    {
        public string Title;
        public string Text;
        public Sprite SmallIcon;
        public Sprite LargeIcon;
        public float HoursAfterExit; // Время в часах после выхода из игры, через которое должно прийти уведомление
    }

    public string ChannelId = "important_channel";
    public List<NotificationContent> Notifications;

    public List<AndroidNotification> ToAndroidNotifications()
    {
        List<AndroidNotification> androidNotifications = new List<AndroidNotification>();
        foreach (var content in Notifications)
        {
            var fireTime = DateTime.Now.AddHours(content.HoursAfterExit);
            var notification = new AndroidNotification()
            {
                Title = content.Title,
                Text = content.Text,
                SmallIcon = ConvertSpriteToAndroidIcon(content.SmallIcon),
                LargeIcon = ConvertSpriteToAndroidIcon(content.LargeIcon),
                FireTime = fireTime
            };
            androidNotifications.Add(notification);
        }
        return androidNotifications;
    }

    private string ConvertSpriteToAndroidIcon(Sprite sprite)
    {
        // Ваша логика для конвертации Sprite в путь или имя ресурса
        return sprite ? sprite.name : "default_icon";
    }
}