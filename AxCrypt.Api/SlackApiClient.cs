﻿using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Rest;
using AxCrypt.Api.Model;
using AxCrypt.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Api
{
    public class SlackApiClient
    {
        private ApiCaller Caller { get; } = new ApiCaller();

        private Uri _accessTokenUrl;

        private string _userName;

        private string _channel;

        public SlackApiClient(Uri accessTokenUrl, string userName, string channel)
        {
            _accessTokenUrl = accessTokenUrl;
            _userName = userName;
            _channel = channel;
        }

        public SlackApiClient(Uri accessTokenUrl, string userName)
            : this(accessTokenUrl, userName, String.Empty)
        {
        }

        public SlackApiClient(Uri accessTokenUrl)
            : this(accessTokenUrl, "AxCrypt Web", String.Empty)
        {
        }

        public async Task PostMessageAsync(string text)
        {
            SlackPayload payload = new SlackPayload()
            {
                Channel = _channel,
                UserName = _userName,
                Text = text,
            };

            RestResponse restResponse;
            try
            {
                RestContent content = new RestContent(New<IStringSerializer>().Serialize(payload));
                restResponse = await Caller.RestAsync(new RestIdentity(), new RestRequest("POST", _accessTokenUrl, TimeSpan.Zero, content)).Free();
                ApiCaller.EnsureStatusOk(restResponse);
            }
            catch (Exception ex)
            {
                New<IReport>().Exception(ex);
            }
        }
    }
}