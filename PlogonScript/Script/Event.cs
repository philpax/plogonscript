using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace PlogonScript.Script;

public class Event
{
    public Event(string name, params (string, Type)[] arguments)
    {
        Name = name;
        Arguments = ToDictionary(arguments);
    }

    private static Dictionary<string, ValueType> ToDictionary<ValueType>(IEnumerable<(string, ValueType)>? arguments)
    {
        return arguments?.ToDictionary(t => t.Item1, t => t.Item2) ?? new Dictionary<string, ValueType>();
    }

    public string Name { get; }
    public Dictionary<string, Type> Arguments { get; }

    public void Call(Script script, params (string, object)[] arguments)
    {
        var args = ToDictionary(arguments);

        if (args.Any(kv => !(Arguments.ContainsKey(kv.Key) && args[kv.Key].GetType() == Arguments[kv.Key])))
            throw new VerificationException("failed to match arguments for event");

        script.CallGlobalFunction(Name, args);
    }
}