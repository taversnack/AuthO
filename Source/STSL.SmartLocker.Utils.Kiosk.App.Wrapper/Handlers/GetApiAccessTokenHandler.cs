using STSL.SmartLocker.Utils.Kiosk.Models;
using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Contracts;
using System;
using System.Threading.Tasks;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Handlers;

public class GetApiAccessTokenHandler : IWebRequestHandler
{
    private readonly IAccessTokenService _accessTokenService;

    public GetApiAccessTokenHandler(IAccessTokenService accessTokenService)
    {
        _accessTokenService = accessTokenService;
    }

    public Task<Response> HandleAsync(object data)
    {
        return GetApiAccessTokenAsync();
    }

    #region Private Methods
    private async Task<Response> GetApiAccessTokenAsync()
    {
        try
        {
            string token = await _accessTokenService.GetAccessTokenAsync();
            return new Response(token, true);
        }
        catch (Exception ex)
        {
            return new Response(null, false, ex.ToString());
        }
    }

    #endregion
}

