using System;
using System.IO;
using System.Windows.Forms;
using TinyMessenger;

namespace AudibleBookmarks
{
    class FileDialogService
    {
        private void OpenDialog(OpenFileMessage msg)
        {
            var ofd = new OpenFileDialog();
            ofd.RestoreDirectory = true;
            var result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (msg.OpenStream)
                {
                    using (var stream = ofd.OpenFile())
                    {
                        msg.OpenStreamAction(stream);
                    }
                }
                else
                {
                    msg.PassFileNameAction(ofd.FileName);
                }
            }
        }

        public void StartListening()
        {
            TinyMessengerHub.Instance.Subscribe<OpenFileMessage>(OpenDialog);
        }
    }

    public class OpenFileMessage : TinyMessageBase
    {
        public OpenFileMessage(object sender, Action<Stream> fileSelectedAction) : base(sender)
        {
            OpenStreamAction = fileSelectedAction;
            OpenStream = true;
        }
        public OpenFileMessage(object sender, Action<string> fileSelectedAction) : base(sender)
        {
            PassFileNameAction = fileSelectedAction;
            OpenStream = false;
        }

        public bool OpenStream { get; set; }
        public Action<Stream> OpenStreamAction { get; set; }

        public Action<string> PassFileNameAction { get; set; }
    }
}
