using UnityEngine;
using System.Collections;
using Common;

namespace Notification
{
#if UNITY_IPHONE
using NotificationServices = UnityEngine.iOS.NotificationServices;
using NotificationType = UnityEngine.iOS.NotificationType;
#endif

public static class LocalNotification
{
    public static void RegisterPermission()
    {
#if UNITY_IPHONE
        UnityEngine.iOS.NotificationServices.RegisterForNotifications(
            NotificationType.Alert |
            NotificationType.Badge |
            NotificationType.Sound);
#endif
    }

    public static void RegisterNotificationMessage(string title, string message, int hour, int minute, bool isRepeatDay)
    {
        System.DateTime now = System.DateTime.Now;
        int year = now.Year;
        int month = now.Month;
        int day = now.Day;
        System.DateTime newDate = new System.DateTime(year, month, day, hour, minute, 0);
        NotificationMessage(title, message, newDate, isRepeatDay);
    }

    public static void NotificationMessage(string title, string message, System.DateTime newDate, bool isRepeatDay)
    {
#if UNITY_IPHONE
		if(newDate >= System.DateTime.Now)
		{
			UnityEngine.iOS.LocalNotification localNotification = new UnityEngine.iOS.LocalNotification();
			localNotification.fireDate =newDate;	
			localNotification.alertBody = message;
			localNotification.applicationIconBadgeNumber = 1;
			localNotification.hasAction = true;
			localNotification.alertAction = title;
			if(isRepeatDay)
			{
				localNotification.repeatCalendar = UnityEngine.iOS.CalendarIdentifier.ChineseCalendar;
				localNotification.repeatInterval = UnityEngine.iOS.CalendarUnit.Day;
			}
			localNotification.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
			UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(localNotification);
		}
#endif
            
        }

        //清空所有本地消息
        public static void CleanNotification()
    {
#if UNITY_IPHONE
		UnityEngine.iOS.LocalNotification l = new UnityEngine.iOS.LocalNotification (); 
		l.applicationIconBadgeNumber = -1; 
		UnityEngine.iOS.NotificationServices.PresentLocalNotificationNow (l); 
		UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications (); 
		UnityEngine.iOS.NotificationServices.ClearLocalNotifications (); 
#endif
// #if UNITY_ANDROID
// 		LocalNotification.CancelNotification(1);
// 		LocalNotification.CancelNotification(2);
// #endif
    }
}

}