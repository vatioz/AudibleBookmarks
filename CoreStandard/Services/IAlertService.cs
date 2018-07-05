using System;

namespace AudibleBookmarks.Core.Services
{
    public interface IAlertService
    {
        void ShowAlert(Exception msg);
    }
}