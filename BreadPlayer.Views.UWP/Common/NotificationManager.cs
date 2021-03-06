﻿using BreadPlayer.Core.Common;
using BreadPlayer.Core.Models;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Data.Xml.Dom;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace BreadPlayer.NotificationManager
{
    public class BreadNotificationManager : ObservableObject, INotificationManager
    {
        private DispatcherTimer _hideTimer;
        private string _status = "Nothing Baking";

        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        private bool _show;

        public bool Show
        {
            get => _show;
            set => Set(ref _show, value);
        }

        public async Task ShowMessageAsync(string status, int duration = 10)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Status = status;
                Show = true;
                if (duration > 0)
                {
                    _hideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(duration) };
                    _hideTimer.Start();
                    _hideTimer.Tick += HideTimer_Tick;
                }
            });
        }

        public async Task ShowMessageBoxAsync(string message, string title)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var dialog = new MessageDialog(message, title);
                await dialog.ShowAsync();
            });
        }

        public void SendUpcomingSongNotification(Mediafile mediaFile)
        {
            if (mediaFile != null)
            {
                ToastNotificationManager.History.Clear();
                var notificationXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);
                IXmlNode toastNode = notificationXml.SelectSingleNode("/toast");
                ((XmlElement)toastNode).SetAttribute("launch", "NowPlaying.xaml");

                XmlElement audio = notificationXml.CreateElement("audio");
                audio.SetAttribute("silent", "True");  //Here
                toastNode.AppendChild(audio);
                var toeastElement = notificationXml.GetElementsByTagName("text");
                toeastElement[0].AppendChild(notificationXml.CreateTextNode("Upcoming Song"));
                toeastElement[1].AppendChild(notificationXml.CreateTextNode(mediaFile.Title));
                toeastElement[2].AppendChild(notificationXml.CreateTextNode(mediaFile.LeadArtist));
                var imageElement = notificationXml.GetElementsByTagName("image");
                imageElement[0].Attributes[1].NodeValue = mediaFile.AttachedPicture ?? "ms-appx:///Assets/albumart.png";
                var toastNotification = new ToastNotification(notificationXml)
                {
                    Group = "upcoming-song"
                };

                ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
            }
        }

        private void HideTimer_Tick(object sender, object e)
        {
            Status = "Nothing Baking!";
            Show = false;
            _hideTimer.Stop();
        }
    }
}