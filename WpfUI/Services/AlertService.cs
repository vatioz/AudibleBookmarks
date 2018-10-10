using AudibleBookmarks.Core.Messenger;
using AudibleBookmarks.Core.Services;
using System;
using System.Windows.Forms;

namespace AudibleBookmarks.Services
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
            MessageBox.Show(ex.InnerException?.Message ?? "Error", ex.Message);
        }
    }
}
