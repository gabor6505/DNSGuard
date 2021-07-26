using DnsClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace OpenResolverChecker
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Logging.LoggerFactory = new DebugLoggerFactory();

            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}