using System;
using System.IO;

namespace AudibleBookmarks.Core.Messenger
{
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