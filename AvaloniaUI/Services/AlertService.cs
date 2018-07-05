using AudibleBookmarks.Core.Messenger;
using AudibleBookmarks.Core.Services;
using System;

namespace AvaloniaUI.Services
{
    public class AlertService : ISubscribable, IAlertService
    {
        public void StartListening()
        {
            TinyMessengerHub.Instance.Subscribe<GenericTinyMessage<Exception>>(ShowAlert);
        }

        private void ShowAlert(GenericTinyMessage<Exception> msg)
        {
            ShowAlert(msg.Content);
        }

        public void ShowAlert(Exception ex)
        {
            var mb = new StarDebris.Avalonia.MessageBox.MessageBox(ex.Message);
            mb.Show();
        }
    }
}
