using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebDemo2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder/*.ConfigureKestrel((serverOptions)=> 
                    {
                        serverOptions.Limits.MaxConcurrentConnections = 100;
                        serverOptions.Limits.MaxConcurrentUpgradedConnections = 100;
                        serverOptions.Limits.MaxRequestBodySize = 10 * 1024;
                        serverOptions.Limits.MinRequestBodyDataRate =
                            new MinDataRate(bytesPerSecond: 100,
                                gracePeriod: TimeSpan.FromSeconds(10));
                        serverOptions.Limits.MinResponseDataRate =
                            new MinDataRate(bytesPerSecond: 100,
                                gracePeriod: TimeSpan.FromSeconds(10));
                        serverOptions.Listen(IPAddress.Any, 5000);
                        serverOptions.Listen(IPAddress.Any, 5001,
                            listenOptions =>
                            {
                                listenOptions.UseHttps("testCert.pfx",
                                    "testPassword");
                            });
                        serverOptions.Limits.KeepAliveTimeout =
                            TimeSpan.FromMinutes(2);
                        serverOptions.Limits.RequestHeadersTimeout =
                            TimeSpan.FromMinutes(1);
                    })*/
                    .UseStartup<Startup>();
                });

#if DefaultBuilder
        #region snippet_DefaultBuilder
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        #endregion
#elif TCPSocket
        #region snippet_TCPSocket
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.Listen(IPAddress.Loopback, 5000);
                        serverOptions.Listen(IPAddress.Loopback, 5001, 
                            listenOptions =>
                            {
                                listenOptions.UseHttps("testCert.pfx", 
                                    "testPassword");
                            });
                    })
                    .UseStartup<Startup>();
                });
        #endregion
#elif UnixSocket
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
        #region snippet_UnixSocket
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.ListenUnixSocket("/tmp/kestrel-test.sock");
                        serverOptions.ListenUnixSocket("/tmp/kestrel-test.sock", 
                            listenOptions =>
                            {
                                listenOptions.UseHttps("testCert.pfx", 
                                    "testpassword");
                            });
                    })
        #endregion
                    .UseStartup<Startup>();
                });
#elif FileDescriptor
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseLibuv()
                .ConfigureWebHostDefaults(webBuilder =>
                {
        #region snippet_FileDescriptor
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        var fds = Environment
                            .GetEnvironmentVariable("SD_LISTEN_FDS_START");
                        var fd = ulong.Parse(fds);

                        serverOptions.ListenHandle(fd);
                        serverOptions.ListenHandle(fd, listenOptions =>
                        {
                            listenOptions.UseHttps("testCert.pfx", "testpassword");
                        });
                    })
        #endregion
                    .UseStartup<Startup>();
                });
#elif Limits
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
        #region snippet_Limits
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.Limits.MaxConcurrentConnections = 100;
                        serverOptions.Limits.MaxConcurrentUpgradedConnections = 100;
                        serverOptions.Limits.MaxRequestBodySize = 10 * 1024;
                        serverOptions.Limits.MinRequestBodyDataRate =
                            new MinDataRate(bytesPerSecond: 100, 
                                gracePeriod: TimeSpan.FromSeconds(10));
                        serverOptions.Limits.MinResponseDataRate =
                            new MinDataRate(bytesPerSecond: 100, 
                                gracePeriod: TimeSpan.FromSeconds(10));
                        serverOptions.Listen(IPAddress.Loopback, 5000);
                        serverOptions.Listen(IPAddress.Loopback, 5001, 
                            listenOptions =>
                            {
                                listenOptions.UseHttps("testCert.pfx", 
                                    "testPassword");
                            });
                        serverOptions.Limits.KeepAliveTimeout = 
                            TimeSpan.FromMinutes(2);
                        serverOptions.Limits.RequestHeadersTimeout = 
                            TimeSpan.FromMinutes(1);
                    })
        #endregion
                    .UseStartup<Startup>();
                });
#elif Port0
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
        #region snippet_Port0
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.Listen(IPAddress.Loopback, 0);
                    })
        #endregion
                    .UseStartup<Startup>();
                });
#elif SyncIO
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
        #region snippet_SyncIO
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.AllowSynchronousIO = true;
                    })
        #endregion
                    .UseStartup<Startup>();
                });
#endif
    }
}
