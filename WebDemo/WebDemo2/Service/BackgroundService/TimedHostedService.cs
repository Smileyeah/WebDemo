using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebDemo2.Service.BackgroundService
{
    public class TimedHostedService : ScheduledService
    {
        private int executionCount = 0;
        public TimedHostedService(ILogger<TimedHostedService> _logger) : base(TimeSpan.FromDays(1), _logger)
        { }

        protected override Task ExecuteAsync()
        {
            var count = Interlocked.Increment(ref executionCount);

            base.Logger.LogInformation(
                "Timed Hosted Service is working. Days: {Count}", count);

            return Task.CompletedTask;
        }
    }
}
