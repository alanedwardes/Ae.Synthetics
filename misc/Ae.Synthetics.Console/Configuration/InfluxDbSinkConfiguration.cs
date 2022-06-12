namespace Ae.Synthetics.Console.Configuration
{
    public sealed class InfluxDbSinkConfiguration
    {
        public string Url { get; set; }
        public string Bucket { get; set; }
        public string Org { get; set; }
        public string Token { get; set; }
    }
}