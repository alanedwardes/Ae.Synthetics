using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Synthetics.Alerting.Ses
{
    internal sealed class SesSyntheticsAlerter : ISyntheticsAlerter
    {
        private readonly IAmazonSimpleEmailService _simpleEmailService;
        private readonly SesSyntheticsAlerterConfig _config;

        public SesSyntheticsAlerter(IAmazonSimpleEmailService simpleEmailService, SesSyntheticsAlerterConfig config)
        {
            _simpleEmailService = simpleEmailService;
            _config = config;
        }

        public async Task Failure(IReadOnlyList<string> logEntries, string source, Exception exception, TimeSpan time, CancellationToken token)
        {
            var html = new StringBuilder();
            html.AppendLine($"<p>Synthetic test <b>{source}</b> failed with the following exception, in <b>{time.TotalSeconds}s</b>:</p>");
            html.AppendLine($"<pre>{exception.Demystify()}</pre>");

            if (logEntries.Any())
            {
                html.AppendLine($"<p>Log entries before exception:</p>");
                html.AppendLine($"<ol>");
                foreach (var logEntry in logEntries)
                {
                    html.AppendLine($"<li>{logEntry}</li>");
                }
                html.AppendLine($"</ol>");
            }

            var request = new SendEmailRequest
            {
                Source = _config.Sender,
                Destination = new Destination { ToAddresses = _config.Recipients.ToList() },
                Message = new Message
                {
                    Subject = new Content($"Synthetic Test Failure: {source}"),
                    Body = new Body
                    {
                        Html = new Content(html.ToString())
                    }
                }
            };

            await _simpleEmailService.SendEmailAsync(request, token);
        }

        public Task Success(IReadOnlyList<string> logEntries, string source, TimeSpan time, CancellationToken token) => Task.CompletedTask;
    }
}
