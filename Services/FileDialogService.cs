using System;
using System.IO;
using System.Windows.Forms;

namespace AudibleBookmarks.Services
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

        private void SaveDialog(SaveFileMessage msg)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "Text Files (*.txt)|*.txt";
            dlg.DefaultExt = "txt";
            dlg.AddExtension = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (msg.OpenStream)
                {
                    using (var stream = dlg.OpenFile())
                    {
                        msg.OpenStreamAction(stream);
                    }
                }
                else
                {
                    msg.PassFileNameAction(dlg.FileName);
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

    public class SaveFileMessage : TinyMessageBase
    {
        public SaveFileMessage(object sender, Action<Stream> fileSelectedAction) : base(sender)
        {
            OpenStreamAction = fileSelectedAction;
            OpenStream = true;
        }
        public SaveFileMessage(object sender, Action<string> fileSelectedAction) : base(sender)
        {
            PassFileNameAction = fileSelectedAction;
            OpenStream = false;
        }

        public bool OpenStream { get; set; }
        public Action<Stream> OpenStreamAction { get; set; }

        public Action<string> PassFileNameAction { get; set; }
    }
}
