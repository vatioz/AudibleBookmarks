using System;
using System.IO;

namespace AudibleBookmarks.Core.Messenger
{
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