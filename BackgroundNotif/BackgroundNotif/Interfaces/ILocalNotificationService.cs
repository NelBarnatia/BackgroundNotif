using System;

namespace BackgroundNotif.Interfaces
{
    public interface ILocalNotificationService
    {
        void LocalNotification(string title, string body, int id, DateTime notifyTime);
        void Cancel(int id);
        void CancelAll();
    }
}
