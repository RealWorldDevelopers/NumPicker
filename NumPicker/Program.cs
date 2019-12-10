using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;


namespace NumPicker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CreateWebHostBuilder(args).Build().Run();
            //BuildWebHost(args).Run();
             CreateHostBuilder(args).Build().Run();
        }

        // original with no key vault
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        // Core < v3 
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((ctx, builder) =>
              {
                  var keyVaultEndpoint = GetKeyVaultEndpoint();
                  if (!string.IsNullOrEmpty(keyVaultEndpoint))
                  {
                      var azureServiceTokenProvider = new AzureServiceTokenProvider();
                      var keyVaultClient = new KeyVaultClient(
                          new KeyVaultClient.AuthenticationCallback(
                              azureServiceTokenProvider.KeyVaultTokenCallback));
                      builder.AddAzureKeyVault(
                          keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
                  }
              }
           ).UseStartup<Startup>()
            .Build();

        // Core > v3
        public static IHostBuilder CreateHostBuilder(string[] args) =>
                 Host.CreateDefaultBuilder(args)
                     .ConfigureAppConfiguration((context, config) =>
                     {
                         var keyVaultEndpoint = GetKeyVaultEndpoint();
                         if (!string.IsNullOrEmpty(keyVaultEndpoint))
                         {
                             var azureServiceTokenProvider = new AzureServiceTokenProvider();
                             var keyVaultClient = new KeyVaultClient(
                                 new KeyVaultClient.AuthenticationCallback(
                                     azureServiceTokenProvider.KeyVaultTokenCallback));
                             config.AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
                         }
                     })
                     .ConfigureWebHostDefaults(webBuilder =>
                     {
                         webBuilder.UseStartup<Startup>();
                     });

        private static string GetKeyVaultEndpoint() => "https://RWD-Secrets.vault.azure.net";

    }
}
