using Microsoft.Web.WebView2.Wpf;
using Microsoft.Xaml.Behaviors;
using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.ViewModel;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Behaviors;

public class WebViewPostMessageBehavior : Behavior<WebView2>
{
    private MainWindowViewModel viewModel;

    protected override void OnAttached()
    {
        base.OnAttached();
        viewModel = (MainWindowViewModel)AssociatedObject.DataContext;
        viewModel.WebMessagePosted += ViewModel_WebMessagePosted;
    }
    private void ViewModel_WebMessagePosted(object sender, string e)
    {
        AssociatedObject.CoreWebView2.PostWebMessageAsJson(e);
    }
    protected override void OnDetaching()
    {
        viewModel.WebMessagePosted -= ViewModel_WebMessagePosted;
        base.OnDetaching();
    }
}
