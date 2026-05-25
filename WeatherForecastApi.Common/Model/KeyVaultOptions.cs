namespace WeatherForecastApi.Common.Model;

public sealed class KeyVaultOptions
{
    public const string SectionName = "KeyVault";

    public string VaultUri { get; set; } = "";
    public string DbSecretName { get; set; } = "";
    public string AppSecretName { get; set; } = "";
}
