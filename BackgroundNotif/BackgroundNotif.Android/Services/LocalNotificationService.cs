using System;
using System.IO;
using System.Xml.Serialization;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Support.V4.App;
using BackgroundNotif.Droid.Services;
using BackgroundNotif.Interfaces;
using BackgroundNotif.Models;
using Java.Lang;
using AndroidApp = Android.App.Application;

[assembly: Xamarin.Forms.Dependency(typeof(LocalNotificationService))]
namespace BackgroundNotif.Droid.Services
{
    #region Service
    public class LocalNotificationService : ILocalNotificationService
    {
        int _notificationIconId { get; set; }
        readonly DateTime _jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal string _randomNumber;

        public void LocalNotification(string title, string body, int id, DateTime notifyTime)
        {
            //long repeateDay = 1000 * 60 * 60 * 24;    
            long repeateForMinute = 10000; // In milliseconds   
            long totalMilliSeconds = (long)(notifyTime.ToUniversalTime() - _jan1st1970).TotalMilliseconds;
            if (totalMilliSeconds < JavaSystem.CurrentTimeMillis())
                totalMilliSeconds += repeateForMinute;

            var intent = CreateIntent(id);
            var localNotification = new LocalNotification
            {
                Title = title,
                Body = body,
                Id = id,
                NotifyTime = notifyTime
            };

            if (_notificationIconId != 0)
                localNotification.IconId = _notificationIconId;
            else
                localNotification.IconId = Resource.Mipmap.icon;

            var serializedNotification = SerializeNotification(localNotification);
            intent.PutExtra(ScheduledAlarmHandler.LocalNotificationKey, serializedNotification);

            Random generator = new Random();
            _randomNumber = generator.Next(100000, 999999).ToString("D6");

            var pendingIntent = PendingIntent.GetBroadcast(AndroidApp.Context, Convert.ToInt32(_randomNumber), intent, PendingIntentFlags.Immutable);
            var alarmManager = GetAlarmManager();
            alarmManager.SetRepeating(AlarmType.RtcWakeup, totalMilliSeconds, repeateForMinute, pendingIntent);
        }

        public void Cancel(int id)
        {
            var intent = CreateIntent(id);
            var pendingIntent = PendingIntent.GetBroadcast(AndroidApp.Context, Convert.ToInt32(_randomNumber), intent, PendingIntentFlags.Immutable);
            var alarmManager = GetAlarmManager();
            alarmManager.Cancel(pendingIntent);
            var notificationManager = NotificationManagerCompat.From(AndroidApp.Context);
            //notificationManager.CancelAll();
            notificationManager.Cancel(id);
        }

        public void CancelAll()
        {
            var intent = new Intent(AndroidApp.Context, typeof(ScheduledAlarmHandler));
            var pendingIntent = PendingIntent.GetBroadcast(AndroidApp.Context, Convert.ToInt32(_randomNumber), intent, PendingIntentFlags.Immutable);
            var alarmManager = GetAlarmManager();
            alarmManager.Cancel(pendingIntent);
            var notificationManager = NotificationManagerCompat.From(AndroidApp.Context);
            notificationManager.CancelAll();
        }

        public static Intent GetLauncherActivity()
        {

            var packageName = AndroidApp.Context.PackageName;
            return AndroidApp.Context.PackageManager.GetLaunchIntentForPackage(packageName);
        }


        private Intent CreateIntent(int id)
        {
            return new Intent(AndroidApp.Context, typeof(ScheduledAlarmHandler))
                .SetAction("LocalNotifierIntent" + id);
        }

        private AlarmManager GetAlarmManager()
        {
            var alarmManager = AndroidApp.Context.GetSystemService(Context.AlarmService) as AlarmManager;
            return alarmManager;
        }

        private string SerializeNotification(LocalNotification notification)
        {
            var xmlSerializer = new XmlSerializer(notification.GetType());

            using (var stringWriter = new StringWriter())
            {
                xmlSerializer.Serialize(stringWriter, notification);
                return stringWriter.ToString();
            }
        }
    }
    #endregion

    #region Broadcast Reciever
    [BroadcastReceiver(Enabled = true, Label = "Local Notifications Broadcast Receiver")]
    public class ScheduledAlarmHandler : BroadcastReceiver
    {

        public const string LocalNotificationKey = "LocalNotification";
        const string channelId = "default";

        public override void OnReceive(Context context, Intent intent)
        {
            var extra = intent.GetStringExtra(LocalNotificationKey);
            var notification = DeserializeNotification(extra);

            //Generating notification    
            var builder = new NotificationCompat.Builder(AndroidApp.Context, channelId)
                .SetContentTitle(notification.Title)
                .SetContentText(notification.Body)
                .SetSmallIcon(Resource.Mipmap.icon)
                .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Ringtone))
                .SetAutoCancel(true);

            var resultIntent = LocalNotificationService.GetLauncherActivity();
            resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            var stackBuilder = Android.Support.V4.App.TaskStackBuilder.Create(AndroidApp.Context);
            stackBuilder.AddNextIntent(resultIntent);

            Random random = new Random();
            int randomNumber = random.Next(9999 - 1000) + 1000;

            var resultPendingIntent =
                stackBuilder.GetPendingIntent(randomNumber, (int)PendingIntentFlags.Immutable);
            builder.SetContentIntent(resultPendingIntent);

            // Sending notification    
            var notificationManager = NotificationManagerCompat.From(AndroidApp.Context);
            notificationManager.Notify(randomNumber, builder.Build());
        }

        private LocalNotification DeserializeNotification(string notificationString)
        {
            var xmlSerializer = new XmlSerializer(typeof(LocalNotification));
            using var stringReader = new StringReader(notificationString);
            var notification = (LocalNotification)xmlSerializer.Deserialize(stringReader);

            return notification;
        }
    }
    #endregion
}