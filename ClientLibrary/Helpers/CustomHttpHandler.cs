﻿using BaseLibrary.DTOs;
using ClientLibrary.Services.Contracts;
using System.Net;

namespace ClientLibrary.Helpers
{
    public class CustomHttpHandler(
        GetHttpClient getHttpClient,
        LocalStorageService localStorageService,
        IUserAccountService userAccountService
        ) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool loginUrl = request.RequestUri!.AbsoluteUri.Contains("login");
            bool registerUrl = request.RequestUri!.AbsoluteUri.Contains("register");
            bool refreshTokenUrl = request.RequestUri!.AbsoluteUri.Contains("refresh-token");

            if (loginUrl || registerUrl || refreshTokenUrl) return await base.SendAsync(request, cancellationToken);

            var result = await base.SendAsync(request, cancellationToken);
            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                //Get token from local storage
                var stringToken = await localStorageService.GetToken();
                if (stringToken == null) return result;

                //Check if the header contains the token
                string token = string.Empty;
                try { token = request.Headers.Authorization!.Parameter!; }
                catch { }
                var deserializedToken = Serializations.DeserializeJsonString<UserSession>(stringToken);
                if (deserializedToken == null) return result;

                if (string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", deserializedToken.Token);
                    return await base.SendAsync(request, cancellationToken);
                }

                //Check for refresh token
                var newJwtToken = await GetRefreshToken(deserializedToken.RefreshToken!);
                if (string.IsNullOrEmpty(newJwtToken)) return result;

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newJwtToken);
                return await base.SendAsync(request, cancellationToken);
            }
            return result;
        }

        private async Task<string> GetRefreshToken(string refreshToken)
        {
            var result = await userAccountService.RefreshTokenAsync(new RefreshToken { Token = refreshToken });
            string serializedtoken = Serializations.SerializeObj(new UserSession()
            { Token = result.Token, RefreshToken = result.RefreshToken });
            await localStorageService.SetToken(serializedtoken);
            return result.Token;
        }
    }
}
