using System;

namespace Ae.Synthetics.Console.Configuration
{
    public sealed class HttpSyntheticTestConfiguration
    {
        public string Name { get; set; }
        public string Method { get; set; } = "GET";
        public Uri RequestUri { get; set; }
        public int ExpectedStatusCode { get; set; }
    }
}

