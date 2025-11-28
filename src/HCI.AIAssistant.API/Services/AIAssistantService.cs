using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI.Assistants;

namespace HCI.AIAssistant.API.Services;

public class AIAssistantService : IAIAssistantService
{
    private const int _DELAY_IN_MS = 500;

    private readonly ISecretsService _secretsService;
    private readonly AssistantsClient? _assistantsClient;
    private readonly string? _id;

    public AIAssistantService(ISecretsService secretsService)
    {
        _secretsService = secretsService;

        var endPoint = _secretsService.AIAssistantSecrets?.EndPoint;
        var key = _secretsService.AIAssistantSecrets?.Key;
        _id = _secretsService.AIAssistantSecrets?.Id;
        if (string.IsNullOrWhiteSpace(endPoint) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(_id))
        {
            _assistantsClient = null;
            return;
        }

        _assistantsClient = new AssistantsClient(
            new Uri(endPoint),
            new AzureKeyCredential(key)
        );
    }

    public async Task<string> SendMessageAndGetResponseAsync(string message)
    {
        if (_assistantsClient == null || _id == null)
        {
            return "Error: AssistantsClient service is not initialized or assistant id is missing!";
        }

        AssistantThread assistantThread = await _assistantsClient.CreateThreadAsync();
        ThreadMessage threadMessage = await _assistantsClient.CreateMessageAsync(assistantThread.Id, MessageRole.User, message);
        ThreadRun threadRun = await _assistantsClient.CreateRunAsync(assistantThread.Id, new CreateRunOptions(_id));

        do
        {
            threadRun = await _assistantsClient.GetRunAsync(assistantThread.Id, threadRun.Id);
            await Task.Delay(TimeSpan.FromMilliseconds(_DELAY_IN_MS));
        }
        while (threadRun.Status == RunStatus.Queued || threadRun.Status == RunStatus.InProgress);

        if (threadRun.Status != RunStatus.Completed)
        {
            _ = _assistantsClient.DeleteThreadAsync(assistantThread.Id);
            return "Error!";
        }

        PageableList<ThreadMessage> messagesList = await _assistantsClient.GetMessagesAsync(assistantThread.Id);
        ThreadMessage? lastAssistantMessage = messagesList.FirstOrDefault(
            m => m.Role == MessageRole.Assistant
        );
        if (lastAssistantMessage?.ContentItems?.FirstOrDefault() is not MessageTextContent messageTextContent)
        {
            _ = _assistantsClient.DeleteThreadAsync(assistantThread.Id);
            return "Error!";
        }

        _ = _assistantsClient.DeleteThreadAsync(assistantThread.Id);
        return messageTextContent.Text;
    }
}