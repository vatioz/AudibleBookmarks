
using System;
using System.Windows.Forms;
using TinyMessenger;

namespace AudibleBookmarks
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
