using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FindMultiLabelledEmails
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            // tell the builder to look for the appsettings.json file
            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.AddUserSecrets<Program>();

            Configuration = builder.Build();

            IServiceCollection services = new ServiceCollection();

            //Map the implementations of your classes here ready for DI
            services
                .Configure<GoogleSecrets>(Configuration.GetSection("Google"))
                .AddOptions()
                .AddScoped<IFinder, Finder>()
                .BuildServiceProvider();

            var serviceProvider = services.BuildServiceProvider();

            var finder = serviceProvider.GetService<IFinder>();

            finder.Find();
        }
    }

}
