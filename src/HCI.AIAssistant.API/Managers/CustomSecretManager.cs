using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

namespace HCI.AIAssistant.API.Managers;

public class CustomSecretManager : KeyVaultSecretManager
{
    private readonly string _prefix;
    public CustomSecretManager(string prefix)
    {
        _prefix = $"{prefix}-";
    }
    public override bool Load(SecretProperties secret)
    => secret.Name.StartsWith(_prefix);
    public override string GetKey(KeyVaultSecret secret)
    => secret.Name[_prefix.Length..].Replace("--",
   ConfigurationPath.KeyDelimiter);
}

