using System.Threading.Tasks;
using System;
using System.Net.Http;
using Ae.Synthetics.Runner;
using System.Threading;
using Microsoft.Extensions.Logging;
using System.Net;
using Ae.Synthetics.Console.Configuration;
using System.Diagnostics;

namespace Ae.Synthetics.Console
{
    public sealed class HttpSyntheticTest : ISyntheticTest
    {
        private sealed class HttpSyntheticTestException : Exception
        {
            public HttpSyntheticTestException(string message) : base(message)
            {
            }
        }

        public const string CLIENT_NAME = "SYNTHETICS";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpSyntheticTestConfiguration _configuration;

        public string Name => _configuration.Name;

        public HttpSyntheticTest(IHttpClientFactory httpClientFactory, HttpSyntheticTestConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task Run(ILogger logger, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();

            logger.LogInformation("Making request to {HttpMethod} {RequestUri}", _configuration.Method, _configuration.RequestUri);

            using var client = _httpClientFactory.CreateClient(CLIENT_NAME);

            using var request = new HttpRequestMessage
            {
                Method = new HttpMethod(_configuration.Method),
                RequestUri = _configuration.RequestUri
            };

            using var response = await client.SendAsync(request, token);

            if (response.StatusCode != (HttpStatusCode)_configuration.ExpectedStatusCode)
            {
                throw new HttpSyntheticTestException($"Expected status code {_configuration.ExpectedStatusCode}, got {response.StatusCode}");
            }

            logger.LogInformation("Completed request to {HttpMethod} {RequestUri} in {ElapsedMilliseconds}ms", _configuration.Method, _configuration.RequestUri, sw.ElapsedMilliseconds);
        }
    }
}

