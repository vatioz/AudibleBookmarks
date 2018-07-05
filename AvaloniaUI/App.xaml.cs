using Avalonia;
using Avalonia.Markup.Xaml;

namespace AvaloniaUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
