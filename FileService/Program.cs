using System;
using System.IO;
using System.Threading;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.Extensions.DependencyInjection;

namespace FileService
{
	public class Program
	{
		static void Main(string[] args)
		{
            var services = new ServiceCollection();

            services.Configure<DotNetFileSystemOptions>(opt => opt
                .RootPath = Path.Combine(Path.GetTempPath(), "TestFtpServer"));

            services.AddFtpServer(builder => builder
                .UseDotNetFileSystem()
                .EnableAnonymousAuthentication());

            services.Configure<FtpServerOptions>(opt => opt.ServerAddress = "10.93.9.117");

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var ftpServerHost = serviceProvider.GetRequiredService<IFtpServerHost>();

                ftpServerHost.StartAsync(CancellationToken.None).Wait();

                Console.WriteLine("Press ENTER/RETURN to close the test application.");
                Console.ReadLine();

                ftpServerHost.StopAsync(CancellationToken.None).Wait();
            }
		}
	}
}
