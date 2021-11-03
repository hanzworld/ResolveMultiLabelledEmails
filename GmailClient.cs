using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace FindMultiLabelledEmails
{
    public class GmailClient : GmailService
    {
        public GmailClient(HttpClient client) : base(
            CreateBaseObject(
                client.DefaultRequestHeaders.GetValues(Constants.SecretsInHeadersHack.Id).First(),
                client.DefaultRequestHeaders.GetValues(Constants.SecretsInHeadersHack.Secret).First()
            )
        )
        {
        }

        private static Initializer CreateBaseObject(string id, string secret)
        {
            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets()
                {
                    ClientId = id,
                    ClientSecret = secret
                },
                new[] { Scope.GmailReadonly },
                "user",
                CancellationToken.None,
                new FileDataStore("Books.ListMyLibrary")).Result;

            return new Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "FindMultiLabelledEmails",
            };
        }
    }
}