using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GmailToIMAPMigration.ResolveMultiLabelledEmails
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
                .AddScoped<IResolver, Resolver>();

            services.AddHttpClient<GmailClient>((serviceProvider, client) =>
            {
                /* 
                this is an incredibly ugly hack but I have Google'd for two days straight and 
                can't figure out how this should be done properly. If you know, I'd love to hear from you!
                */
                var settings = serviceProvider.GetRequiredService<IOptions<GoogleSecrets>>().Value;
                /*
                to pass in the secrets to the third party Google constructor, we'll add 
                them as headers to the client (in full knowledge that this client is disposed 
                during construction of the GmailClient - see GmailClient implementation), 
                and then retrieve them on the other side. Ick.
                */
                client.DefaultRequestHeaders.Add(Constants.SecretsInHeadersHack.Id, settings.ClientId);
                client.DefaultRequestHeaders.Add(Constants.SecretsInHeadersHack.Secret, settings.ClientSecret);
            });

            var serviceProvider = services.BuildServiceProvider();

            var resolver = serviceProvider.GetService<IResolver>();
            resolver.Resolve();
        }
    }

}
