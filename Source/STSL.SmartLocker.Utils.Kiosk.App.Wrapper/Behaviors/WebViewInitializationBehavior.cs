using Microsoft.Extensions.Configuration;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Xaml.Behaviors;
using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.ViewModel;
using System.Threading.Tasks;
using System.Windows;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Behaviors;

public class WebViewInitializationBehavior : Behavior<WebView2>
{
    private MainWindowViewModel viewModel;

    public WebViewInitializationBehavior()
    {

    }

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.Loaded += AssociatedObject_Loaded;
        viewModel = (MainWindowViewModel)AssociatedObject.DataContext;
    }

    private async void AssociatedObject_Loaded(object sender, RoutedEventArgs e) => await InitializeWebView();
    private async Task InitializeWebView()
    {
        var config = App.GetService<IConfiguration>();
        var webClient = new WebClient(config);
        config!.Bind(WebClient.Name, webClient);
        var url = webClient.ContentUrl;
        await AssociatedObject.EnsureCoreWebView2Async(null);
        AssociatedObject.CoreWebView2.SetVirtualHostNameToFolderMapping(hostName: "vueapp", folderPath: "", accessKind: CoreWebView2HostResourceAccessKind.Allow);
        AssociatedObject.CoreWebView2.Navigate(url.ToString());
    }
    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= AssociatedObject_Loaded;
        base.OnDetaching();
    }
}