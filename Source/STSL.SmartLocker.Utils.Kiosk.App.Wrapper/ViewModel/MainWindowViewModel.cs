using Microsoft.Extensions.Options;
using Microsoft.Web.WebView2.Core;
using Prism.Commands;
using Prism.Mvvm;
using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Contracts;
using STSL.SmartLocker.Utils.Kiosk.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.ViewModel;

public class MainWindowViewModel : BindableBase
{
    private JsonSerializerOptions serializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    public Dictionary<string, CancellationTokenSource> Events { get; set; } = new Dictionary<string, CancellationTokenSource>();
    public DelegateCommand<CoreWebView2WebMessageReceivedEventArgs> OnWebMessageReceivedCommand { get; set; }
    public event EventHandler<string> WebMessagePosted;
    public MainWindowViewModel()
    {
        OnWebMessageReceivedCommand = new DelegateCommand<CoreWebView2WebMessageReceivedEventArgs>(WebMessageReceived);
    }


    #region WebMessageReceived
    private async void WebMessageReceived(CoreWebView2WebMessageReceivedEventArgs e)
    {
        var message = JsonSerializer.Deserialize<WebViewMessage>(e.WebMessageAsJson, serializerOptions);
        if (message == null)
        {
            return;
        }
        if (message.Type == "request" || message.Type == "subscribe-event")
        {
            var handlerFactory = App.GetRequiredService<IWebRequestHandlerFactory>();
            var handler = handlerFactory.CreateHandler(message!.Handler!);
            await SendDataAsync(message, handler);
        }
        else if (message.Type == "unsubscribe-event")
        {
            if (Events.TryGetValue(message.Handler!, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
                Events.Remove(message.Handler!);
            }
        }
    }
    #endregion

    #region SendDataAsync
    private async Task SendDataAsync(WebViewMessage message, IWebRequestHandler handler)
    {
        await NotifyWebAsync(message, handler);
        await SubscribeEventIfEventRequest(message, handler);
    }

    private async Task SubscribeEventIfEventRequest(WebViewMessage message, IWebRequestHandler handler)
    {
        if (message!.Type == "subscribe-event")
        {
            CancellationTokenSource = new CancellationTokenSource();
            var token = CancellationTokenSource.Token;
            TryCancelExistingEvent(message);
            Events.Add(message!.Handler!, CancellationTokenSource!);
            var settings = App.GetService<IOptions<AppSettings>>();
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(TimeSpan.FromSeconds(settings.Value.StatusDelayTime));
                    token.ThrowIfCancellationRequested();
                    await NotifyWebAsync(message, handler);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }
    }

    private void TryCancelExistingEvent(WebViewMessage message)
    {

        if (Events.TryGetValue(message.Handler!, out var tokenSource))
        {
            tokenSource.Cancel();
            Events.Remove(message!.Handler!);
        }


    }
    #endregion

    private async Task NotifyWebAsync(WebViewMessage message, IWebRequestHandler handler)
    {
        var data = await handler.HandleAsync(message.Data);
        if (data != null)
        {
            data.Handler = message.Handler;
            WebMessagePosted?.Invoke(this, JsonSerializer.Serialize(data, serializerOptions));
        }
    }
    public CancellationTokenSource CancellationTokenSource { get; set; }
}
