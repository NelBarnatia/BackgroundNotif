using System;
using System.Collections.Generic;
using System.Text;

namespace BackgroundNotif.Interfaces
{
    public interface ILocalNotificationService
    {
        void LocalNotification(string title, string body, int id, DateTime notifyTime);
        void Cancel(int id);
    }
}
