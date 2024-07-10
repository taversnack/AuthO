using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.ViewModel;
using System.Windows;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel mainWindowViewModel)
        {
            DataContext = mainWindowViewModel;
            InitializeComponent();
        }
    }
}
