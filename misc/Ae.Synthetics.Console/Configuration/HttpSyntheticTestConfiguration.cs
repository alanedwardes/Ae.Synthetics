using System;
using System.Collections.Generic;

namespace Ae.Synthetics.Console.Configuration
{
    public sealed class HttpSyntheticTestConfiguration
    {
        public string Name { get; set; }
        public string Method { get; set; } = "GET";
        public Uri RequestUri { get; set; }
        public int ExpectedStatusCode { get; set; } = 200;
        public bool AllowAutoRedirect { get; set; } = false;
        public int RetryCount { get; set; } = 0;
        public IDictionary<string, string> ExpectedHeaders { get; set; } = new Dictionary<string, string>();
    }
}