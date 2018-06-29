using System;
using System.Windows.Forms;

namespace AudibleBookmarks.Services
{
    public class AlertService
    {
        public void StartListening()
        {
            TinyMessengerHub.Instance.Subscribe<GenericTinyMessage<Exception>>(ShowAlert);
        }

        private void ShowAlert(GenericTinyMessage<Exception> msg)
        {
            MessageBox.Show(msg.Content.Message);
        }
    }
}
