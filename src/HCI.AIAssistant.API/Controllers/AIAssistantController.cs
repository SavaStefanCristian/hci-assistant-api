using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HCI.AIAssistant.API.Models.DTOs.AIAssistantController;
using HCI.AIAssistant.API.Services;
using HCI.AIAssistant.API.Models.DTOs;
using Microsoft.Azure.Devices;
using System.Text;
using Newtonsoft.Json;

namespace HCI.AIAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIAssistantController : ControllerBase
{
    private readonly ISecretsService _secretsService;
    private readonly IAppConfigurationsService _appConfigurationsService;
    private readonly IAIAssistantService _aIAssistantService;
    private readonly IParametricFunctions _parametricFunctions;

    public AIAssistantController(
        ISecretsService secretsService,
        IAppConfigurationsService appConfigurationsService,
        IAIAssistantService aIAssistantService,
        IParametricFunctions parametricFunctions
    )
    {
        _secretsService = secretsService;
        _appConfigurationsService = appConfigurationsService;
        _aIAssistantService = aIAssistantService;
        _parametricFunctions = parametricFunctions;
    }

    [HttpPost("message")]
    [ProducesResponseType(typeof(AIAssistantControllerPostMessageResponseDTO), 200)]
    [ProducesResponseType(typeof(ErrorResponseDTO), 400)]
    public async Task<ActionResult> PostMessage([FromBody] AIAssistantControllerPostMessageRequestDTO request)
    {
        if (!_parametricFunctions.ObjectExistsAndHasNoNullPublicProperties(request))
        {
            return BadRequest(
                new ErrorResponseDTO()
                {
                    TextErrorTitle = "AtLeastOneNullParameter",
                    TextErrorMessage = "Some parameters are null/missing.",
                    TextErrorTrace = _parametricFunctions.GetCallerTrace()
                }
            );
        }

        if (request.Messages == null || !request.Messages.Any())
        {
            return BadRequest(new ErrorResponseDTO()
            {
                TextErrorTitle = "MissingMessages",
                TextErrorMessage = "The message history is empty.",
                TextErrorTrace = _parametricFunctions.GetCallerTrace()
            });
        }

        StringBuilder promptBuilder = new();

        promptBuilder.AppendLine("Instruction: " + _appConfigurationsService.Instruction + "\nAllowed Users Information: " + _appConfigurationsService.AccessInformation + "\nChat:");

        foreach (var msg in request.Messages)
        {
            string role = msg.SenderType == "User" ? "User" : "Assistant";
            promptBuilder.AppendLine($"{role}: {msg.Text}");
        }

        promptBuilder.AppendLine("Assistant:");

        string fullContextPrompt = promptBuilder.ToString();


        // Console.WriteLine(fullContextPrompt);

#pragma warning disable CS8604
        string textMessageResponse = await _aIAssistantService.SendMessageAndGetResponseAsync(fullContextPrompt);
#pragma warning restore CS8604

        var cmdRegex = new System.Text.RegularExpressions.Regex(@"\[END\]\s*\[(\d+)\]");
        var match = cmdRegex.Match(textMessageResponse);



        AIAssistantControllerPostMessageResponseDTO response = new()
        {
            TextMessage = textMessageResponse
        };

        string? ioTHubConnectionString = _secretsService?.IoTHubSecrets?.ConnectionString;
        if (ioTHubConnectionString != null)
        {
            string numberOnly = match.Groups[1].Value;

            var serviceClientForIoTHub = ServiceClient.CreateFromConnectionString(ioTHubConnectionString);
            var seralizedMessage = JsonConvert.SerializeObject(numberOnly);

            var ioTMessage = new Message(Encoding.UTF8.GetBytes(seralizedMessage));
            await serviceClientForIoTHub.SendAsync(_appConfigurationsService.IoTDeviceName, ioTMessage);
        }

        return Ok(response);
    }
}