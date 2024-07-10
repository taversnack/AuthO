using Microsoft.Extensions.DependencyInjection;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Kiosk.Models;
using System;
using System.Text.Json;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Services
{
    public class PostMessageService : IPostMessageService
    {
        private readonly IServiceProvider _serviceProvider;

        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        public PostMessageService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public void PostMessage(Response message)
        {
            var mainWIndow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWIndow.Dispatcher.Invoke(() =>
            {
                mainWIndow.webView.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(message, _jsonSerializerOptions));
            });
        }
    }
}
