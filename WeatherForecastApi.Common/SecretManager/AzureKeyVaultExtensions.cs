using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using WeatherForecastApi.Common.Model;

namespace WeatherForecastApi.Common.SecretManager;

public static class AzureKeyVaultExtensions
{
    public static IConfigurationBuilder AddAzureKeyVault(
        this IConfigurationBuilder builder,
        KeyVaultOptions options,
        TimeSpan? reloadInterval = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.VaultUri))
        {
            return builder;
        }

        builder.AddAzureKeyVault(
            new Uri(options.VaultUri),
            new DefaultAzureCredential(),
            new AzureKeyVaultConfigurationOptions
            {
                ReloadInterval = reloadInterval ?? TimeSpan.FromHours(1),
            });

        return builder;
    }
}
