using System.Reflection;
using System.Text.RegularExpressions;

namespace STSL.SmartLocker.Utils.Reporting.Data.Deploy.Utils;

internal sealed class ScriptUtils
{
    internal static ScriptFileType ParseScriptType(string scriptName)
    {
        // the scriptName is passed in with the name of assembly and folders with '.' replacing the path separator.
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name?.Replace("/", ".");

        var match = new Regex($"^{assemblyName}\\.Scripts\\.(Schema|Functions|StoredProcedures|Views)\\..+\\.sql",
            RegexOptions.Compiled | RegexOptions.IgnoreCase).Match(scriptName);

        return match.Success ? match.Groups[1].Value.ToLower() switch
        {
            "schema" => ScriptFileType.Schema,
            "functions" => ScriptFileType.Function,
            "storedprocedures" => ScriptFileType.StoredProcedure,
            "views" => ScriptFileType.View,
            _ => throw new ApplicationException($"unexpected script type {match.Value}")
        } : throw new ApplicationException($"failed to parse script {scriptName}");
    }
}
