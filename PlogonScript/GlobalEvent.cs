using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace PlogonScript;

public class GlobalEvent
{
    public GlobalEvent(string name, Dictionary<string, Type>? arguments = null)
    {
        Name = name;
        Arguments = arguments ?? new Dictionary<string, Type>();
    }

    public string Name { get; }
    public Dictionary<string, Type> Arguments { get; }

    public void Call(Script script, Dictionary<string, object>? arguments = null)
    {
        arguments ??= new Dictionary<string, object>();

        if (arguments.Any(kv => !(Arguments.ContainsKey(kv.Key) && arguments[kv.Key].GetType() == Arguments[kv.Key])))
            throw new VerificationException("failed to match arguments for event");

        script.CallGlobalFunction(Name, arguments);
    }
}