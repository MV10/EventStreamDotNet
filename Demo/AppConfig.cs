
using EventStreamDotNet;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Demo
{
    public class AppConfig
    {
        public static AppConfig Get { get; private set; }

        public static void LoadConfiguration()
        {
            Get = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build()
                .Get<AppConfig>(); 
                // oddly, IConfiguration.Get<T> comes from Microsoft.Extensions.Logging...
        }

        public EventStreamDotNetConfig EventStreamDotNet { get; set; }
    }
}
