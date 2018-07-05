using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaUI.ViewModels;
using ReactiveUI;

namespace AvaloniaUI.Views
{
    public class MainWindow : Window //, IViewFor<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        //public MainWindowViewModel ViewModel { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        //object IViewFor.ViewModel { get => ViewModel; set => ViewModel = (MainWindowViewModel)value; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        //public static readonly DependencyProperty ViewModelProperty =
        //DependencyProperty.Register("ViewModel", typeof(TheViewModel), typeof(TheView));
    }
}
