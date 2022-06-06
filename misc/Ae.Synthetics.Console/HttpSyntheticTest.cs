using System.Threading.Tasks;
using System;
using System.Net.Http;
using Ae.Synthetics.Runner;
using System.Threading;
using Microsoft.Extensions.Logging;
using System.Net;
using Ae.Synthetics.Console.Configuration;
using System.Diagnostics;
using System.Linq;

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

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpSyntheticTestConfiguration _configuration;
        private readonly string clientName;

        public string Name => _configuration.Name;

        public HttpSyntheticTest(IHttpClientFactory httpClientFactory, HttpSyntheticTestConfiguration configuration, string clientName)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            this.clientName = clientName;
        }

        public async Task Run(ILogger logger, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();

            logger.LogInformation("Making request to {HttpMethod} {RequestUri}", _configuration.Method, _configuration.RequestUri);

            using var client = _httpClientFactory.CreateClient(clientName);

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

            var headers = response.Headers.Select(x => x).Concat(response.Content.Headers.Select(x => x)).ToArray();
            foreach (var expectedHeader in _configuration.ExpectedHeaders)
            {
                var matchingHeaderValues = headers.Where(x => x.Key == expectedHeader.Key).SelectMany(x => x.Value).ToArray();
                if (matchingHeaderValues.Length == 0)
                {
                    throw new HttpSyntheticTestException($"Expected header {expectedHeader.Key} in response, but it was not present");
                }

                if (!matchingHeaderValues.Any(x => x.Equals(expectedHeader.Value)))
                {
                    throw new HttpSyntheticTestException($"Expected header {expectedHeader.Key} to have value {expectedHeader.Value}, but got '{string.Join(",", matchingHeaderValues)}' instead");
                }
            }

            logger.LogInformation("Completed request to {HttpMethod} {RequestUri} in {ElapsedMilliseconds}ms", _configuration.Method, _configuration.RequestUri, sw.ElapsedMilliseconds);
        }
    }
}

