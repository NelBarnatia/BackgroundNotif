using BackgroundNotif.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace BackgroundNotif.ViewModels
{
    public class BackgroundNotifViewModel : INotifyPropertyChanged
    {

        public BackgroundNotifViewModel()
        {
            SaveCommand = new Command(() => SaveLocalNotification());
            CancelCommand = new Command(() => CancelLocalNotification());
        }

        Command _saveCommand;
        Command cancelCommand;

        public Command CancelCommand
        {
            get
            {
                return cancelCommand;
            }
            set
            {
                SetProperty(ref cancelCommand, value);
            }
        }

        public Command SaveCommand
        {
            get
            {
                return _saveCommand;
            }
            set
            {
                SetProperty(ref _saveCommand, value);
            }
        }

        bool _notificationONOFF;
        public bool NotificationONOFF
        {
            get
            {
                return _notificationONOFF;
            }
            set
            {
                SetProperty(ref _notificationONOFF, value);
                Switch_Toggled();
            }
        }

        void Switch_Toggled()
        {
            if (NotificationONOFF == false)
            {
                MessageText = string.Empty;
                SelectedTime = DateTime.Now.TimeOfDay;
                SelectedDate = DateTime.Today;
                DependencyService.Get<ILocalNotificationService>().Cancel(id);
            }
        }

        DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get
            {
                return _selectedDate;
            }
            set
            {
                SetProperty(ref _selectedDate, value);
            }
        }
        TimeSpan _selectedTime = DateTime.Now.TimeOfDay;
        public TimeSpan SelectedTime
        {
            get
            {
                return _selectedTime;
            }
            set
            {
                SetProperty(ref _selectedTime, value);
            }
        }
        string _messageText;
        public string MessageText
        {
            get
            {
                return _messageText;
            }
            set
            {
                SetProperty(ref _messageText, value);
            }
        }

        int id = 1;

        void SaveLocalNotification()
        {
            if (NotificationONOFF == true)
            {
                var date = (SelectedDate.Date.Month.ToString("00") + "-" + SelectedDate.Date.Day.ToString("00") + "-" + SelectedDate.Date.Year.ToString());
                var time = Convert.ToDateTime(SelectedTime.ToString()).ToString("HH:mm");
                var dateTime = date + " " + time;
                var selectedDateTime = DateTime.ParseExact(dateTime, "MM-dd-yyyy HH:mm", CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(MessageText))
                {
                    DependencyService.Get<ILocalNotificationService>().Cancel(id);
                    DependencyService.Get<ILocalNotificationService>().LocalNotification("Local Notification", MessageText, id, selectedDateTime);
                    App.Current.MainPage.DisplayAlert("LocalNotificationDemo", "Notification details saved successfully ", "Ok");
                }
                else
                    App.Current.MainPage.DisplayAlert("LocalNotificationDemo", "Please enter meassage", "OK");
            }
            else
                App.Current.MainPage.DisplayAlert("LocalNotificationDemo", "Please switch on notification", "OK");
        }

        void CancelLocalNotification()
        {
            DependencyService.Get<ILocalNotificationService>().CancelAll();
            DependencyService.Get<ILocalNotificationService>().Cancel(id);
            App.Current.MainPage.DisplayAlert("LocalNotificationDemo", "Notification details canceled successfully ", "Ok");
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;
            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;
            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
