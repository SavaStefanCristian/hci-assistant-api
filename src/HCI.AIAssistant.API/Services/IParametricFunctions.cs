using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HCI.AIAssistant.API.Services;

public interface IParametricFunctions
{
    bool ObjectExistsAndHasNoNullPublicProperties(object? obj);

    public string GetCallerTrace(
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0);
}
