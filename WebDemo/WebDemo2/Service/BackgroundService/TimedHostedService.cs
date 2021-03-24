using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebDemo2.Service.BackgroundService
{
    public class TimedHostedService : ScheduledService
    {
        private int executionCount = 0;
        public TimedHostedService(ILogger<TimedHostedService> _logger) : base(TimeSpan.FromSeconds(15), _logger)
        { }

        protected override Task ExecuteAsync()
        {
            try
            {
                byte[] bytValue = Encoding.UTF8.GetBytes("wacaca");
                using (SHA512 sha512 = new SHA512CryptoServiceProvider())
                {
                    byte[] retVal = sha512.ComputeHash(bytValue);
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in retVal)
                    {
                        sb.AppendFormat("{0:x2}", b);
                    }

                    base.Logger.LogInformation(
                        "Timed Hosted Service is working. Encrypted: {Count}", sb.ToString());
                }
            }
            catch (Exception ex)
            {
                base.Logger.LogError(ex.ToString());
            }

            return Task.CompletedTask;
        }
    }
}
