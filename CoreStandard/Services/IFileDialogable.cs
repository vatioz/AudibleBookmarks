using AudibleBookmarks.Core.Messenger;

namespace AudibleBookmarks.Core.Services
{
    public interface IFileDialogable
    {
        void OpenDialog(OpenFileMessage msg);
        void SaveDialog(SaveFileMessage msg);
    }
}