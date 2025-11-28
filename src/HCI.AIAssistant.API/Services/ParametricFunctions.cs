using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace HCI.AIAssistant.API.Services;

public class ParametricFunctions : IParametricFunctions
{
    public bool ObjectExistsAndHasNoNullPublicProperties(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        var type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var value = prop.GetValue(obj);
            if (value == null)
            {
                return false;
            }
        }

        return true;
    }

    public string GetCallerTrace(
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
    {
        return $"[{filePath}:{lineNumber}] MemberName: {memberName}";
    }
}
