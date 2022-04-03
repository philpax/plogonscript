using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Dalamud.Logging;
using Dalamud.Plugin;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MicroscriptLoader;

public class Script : IDisposable
{
    private readonly Configuration _configuration;
    private readonly DalamudPluginInterface _pluginInterface;
    private Assembly? _assembly;
    private string _contents = string.Empty;
    private AssemblyLoadContext? _context;
    private readonly Dictionary<string, MethodInfo?> _methods = new();
    private object? _scriptObj;
    private Type? _scriptType;

    public Script(string scriptPath, DalamudPluginInterface pluginInterface, Configuration configuration)
    {
        _pluginInterface = pluginInterface;
        _configuration = configuration;
        ScriptPath = scriptPath;
        LoadContents();
    }

    private string ScriptPath { get; }

    public string Name => Path.GetFileName(ScriptPath);
    public bool Loaded => _context != null;

    public string Contents
    {
        get => _contents;
        set => _contents = value ?? throw new ArgumentNullException(nameof(value));
    }

    public void Dispose()
    {
        Unload();
        GC.SuppressFinalize(this);
    }

    public void LoadContents()
    {
        _contents = File.ReadAllText(ScriptPath);
    }

    public void SaveContents()
    {
        if (_contents.Length == 0)
            return;

        File.WriteAllText(ScriptPath, _contents);
    }

    public void Load()
    {
        try
        {
            LoadContents();
            var syntaxTree = CSharpSyntaxTree.ParseText(_contents);
            var references = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic)
                .Select(a => MetadataReference.CreateFromFile(a.Location)).ToArray();

            var compilation = CSharpCompilation.Create(
                Name,
                new[] {syntaxTree},
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                foreach (var diagnostic in failures)
                    PluginLog.Error("[{0:l}] {1:l}: {2:l}", Name, diagnostic.Id, diagnostic.GetMessage());

                return;
            }

            ms.Seek(0, SeekOrigin.Begin);

            _context = new AssemblyLoadContext(Name, true);
            _assembly = _context.LoadFromStream(ms);
            _scriptType = _assembly.GetType("Script");
            if (_scriptType != null)
            {
                _scriptObj = Activator.CreateInstance(_scriptType);
                _pluginInterface.Inject(_scriptObj!);
            }

            Call("Load");
        }
        catch
        {
            Unload();
            throw;
        }
    }

    public void Unload()
    {
        Call("Unload");

        _context?.Unload();
        _context = null;

        _methods.Clear();
        _scriptType = null;
        _scriptObj = null;
        _assembly = null;
    }

    public void Call(string method, params object[] values)
    {
        if (_scriptType == null || _scriptObj == null) return;

        if (!_methods.ContainsKey(method)) _methods[method] = _scriptType.GetMethod(method);
        _methods[method]?.Invoke(_scriptObj!, values);
    }
}