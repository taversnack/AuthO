using IdentityModel.Client;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Exceptions;
using STSL.SmartLocker.Utils.Kiosk.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Services;

public abstract class AccessTokenBase
{
    protected const string ClaimsNamespace = "https://goto-secure.stsl.co.uk";

    protected readonly ILogger _logger;
    protected readonly AppSettings _settings;
    protected readonly IHttpClientFactory _httpClientFactory;

    // for client credentials API access
    private readonly string _clientCredentialsFilePath;
    private JwtSecurityToken _clientCredentialsToken;

    public AccessTokenBase(
        ILogger logger,
        AppSettings settings,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _settings = settings;
        _httpClientFactory = httpClientFactory;

        _clientCredentialsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"STSL\client_token.txt");

        if (_settings.GeneralSettings.TrustedDevice)
        {
            // read cached the token
            if (File.Exists(_clientCredentialsFilePath))
            {
                string accessToken = File.ReadAllText(_clientCredentialsFilePath);
                _clientCredentialsToken = new JwtSecurityToken(accessToken);

                // check that this token is for the client we expected (using Auth0 naming)
                if (_clientCredentialsToken.Subject != $"{_settings.Authorization.ClientCredentials.ClientId}@clients")
                {
                    // can't use this file, discard
                    _clientCredentialsToken = null;
                }
            }
        }
    }

    protected async Task<HttpClient> CreateApiClientWithClientCredentialsAsync()
    {
        HttpClient apiClient = _httpClientFactory.CreateClient("api");
        apiClient.SetBearerToken(await GetClientCredentialsAccessTokenAsync());
        return apiClient;
    }

    protected async Task<string> GetClientCredentialsAccessTokenAsync()
    {
        if (_clientCredentialsToken != null)
        {
            // check existing token has not expired             
            if (_clientCredentialsToken.ValidTo > DateTime.UtcNow.AddMinutes(5))
            {
                return _clientCredentialsToken.RawData;
            }
        }

        HttpClient client = _httpClientFactory.CreateClient("auth");

        // discover endpoints from metadata
        var disco = await client.GetDiscoveryDocumentAsync(_settings.Authorization.AuthServer);
        if (disco.IsError)
        {
            var problem = new ProblemDetails
            {
                Title = $"Failed to get discovery document from {_settings.Authorization.AuthServer}",
                Detail = disco.Error
            };
            LogApiProblem($"{nameof(GetClientCredentialsAccessTokenAsync)} failed", problem);
            throw new ApiProblemException(problem);
        }

        // request token
        var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = disco.TokenEndpoint,
            ClientId = _settings.Authorization.ClientCredentials.ClientId,
            ClientSecret = _settings.Authorization.ClientCredentials.ClientSecret,
            //Scope = _settings.Authorization.ClientCredentials.Scope,
            Parameters =
            {
                { "audience", _settings.Authorization.Audience }
            }
        });

        if (tokenResponse.IsError)
        {
            var problem = new ProblemDetails
            {
                Title = $"Failed to get token from {disco.TokenEndpoint}",
                Detail = tokenResponse.Error
            };
            LogApiProblem($"{nameof(GetClientCredentialsAccessTokenAsync)} failed", problem);
            throw new ApiProblemException(problem);
        }

        _clientCredentialsToken = new JwtSecurityToken(tokenResponse.AccessToken);

        if (_settings.GeneralSettings.TrustedDevice)
        {
            // cache the token to disk (to reduce Auth0 machine to machine auth token requests, e.g. when in development)
            File.WriteAllText(_clientCredentialsFilePath, _clientCredentialsToken.RawData);
        }

        return _clientCredentialsToken.RawData;
    }

    protected void LogApiProblem(string message, ProblemDetails problemDetails)
    {
        _logger.LogError("Api problem: {message}, title: {title}, status: {status}, detail: {detail}", message, problemDetails?.Title, problemDetails?.Status, problemDetails?.Detail);
    }


    protected async Task ThrowIfProblemsAsync(string methodName, HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            ThrowIfProblems(methodName, response, body);
        }
    }

    protected void ThrowIfProblems(string methodName, HttpResponseMessage response, string body)
    {
        if (!response.IsSuccessStatusCode)
        {
            ProblemDetails problem = JsonConvert.DeserializeObject<ProblemDetails>(body);
            LogApiProblem($"{methodName} failed", problem);
            throw new ApiProblemException(problem);
        }
    }
}