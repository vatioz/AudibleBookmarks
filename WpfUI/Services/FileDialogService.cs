using AudibleBookmarks.Core.Messenger;
using AudibleBookmarks.Core.Services;
using System.Windows.Forms;

namespace AudibleBookmarks.Services
{
    public class FileDialogService: ISubscribable, IFileDialogable
    {
        public void OpenDialog(OpenFileMessage msg)
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

        public void SaveDialog(SaveFileMessage msg)
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
            TinyMessengerHub.Instance.Subscribe<SaveFileMessage>(SaveDialog);
        }
    }
}
