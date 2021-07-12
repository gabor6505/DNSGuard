using System;
using DnsClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace OpenResolverChecker
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var ips = OpenResolverCheckController.ResolveAddressToIpAddresses("dns.google");
            foreach (var ip in ips)
            {
                Console.WriteLine(ip.ToString());
            }

            Logging.LoggerFactory = new DebugLoggerFactory();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}