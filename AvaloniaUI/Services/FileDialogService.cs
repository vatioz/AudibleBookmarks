using AudibleBookmarks.Core.Messenger;
using AudibleBookmarks.Core.Services;
using Avalonia.Controls;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaUI.Services
{
    public class FileDialogService: ISubscribable
    {
        public async void OpenDialog(OpenFileMessage msg)
        {
            var ofd = new OpenFileDialog();
            ofd.AllowMultiple = false;
            var result = await ofd.ShowAsync();
            if (result != null && result.Length == 1)
            {
                if (msg.OpenStream)
                {
                    using (var stream = new FileStream(result.First(), FileMode.Open))
                    {
                        msg.OpenStreamAction(stream);
                    }
                }
                else
                {
                    msg.PassFileNameAction(result.First());
                }
            }
        }

        public async void SaveDialog(SaveFileMessage msg)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExtension = "*.txt";
            //dlg.Filter = "Text Files (*.txt)|*.txt";
            var result = await dlg.ShowAsync(null);
            if (!string.IsNullOrWhiteSpace(result))
            {
                if (msg.OpenStream)
                {
                    using (var stream = new FileStream(result, FileMode.OpenOrCreate))
                    {
                        msg.OpenStreamAction(stream);
                    }
                }
                else
                {
                    msg.PassFileNameAction(result);
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
