using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HCI.AIAssistant.API.Models.DTOs.AIAssistantController;

public class MessageDTO
{
    public string? Text { get; set; }
    public string? SenderType { get; set; }
}