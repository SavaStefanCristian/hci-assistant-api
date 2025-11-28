using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HCI.AIAssistant.API.Models.CustomTypes;

namespace HCI.AIAssistant.API.Services;

public class SecretsService : ISecretsService
{
    public AIAssistantSecrets? AIAssistantSecrets { get; set; }
    public IoTHubSecrets? IoTHubSecrets { get; set; }
}
