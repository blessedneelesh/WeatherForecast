using System.Collections.Concurrent;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Newtonsoft.Json;
using WeatherForecastApi.Common.Model;

namespace WeatherForecastApi.Common.SecretManager;

public static class AzureKeyVaultSecretManager
{
    private static readonly ConcurrentDictionary<string, SecretClient> Clients = new(StringComparer.OrdinalIgnoreCase);

    public static async Task<string> GetSecretAsync(string vaultUri, string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vaultUri))
        {
            throw new ArgumentException("Vault URI is required.", nameof(vaultUri));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Secret name is required.", nameof(name));
        }

        var client = Clients.GetOrAdd(vaultUri, uri => new SecretClient(new Uri(uri), new DefaultAzureCredential()));
        var response = await client.GetSecretAsync(name, cancellationToken: cancellationToken).ConfigureAwait(false);
        return response.Value.Value;
    }

    public static async Task<DatabaseSecret> GetDatabaseSecretAsync(string vaultUri, string secretName, CancellationToken cancellationToken = default)
    {
        var raw = await GetSecretAsync(vaultUri, secretName, cancellationToken).ConfigureAwait(false);
        return JsonConvert.DeserializeObject<DatabaseSecret>(raw)
            ?? throw new InvalidOperationException($"Secret '{secretName}' could not be deserialized into DatabaseSecret.");
    }
}
