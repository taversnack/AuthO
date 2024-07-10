using STSL.SmartLocker.Utils.CLI.Services.Contracts;

namespace STSL.SmartLocker.Utils.CLI.Services;

internal sealed class ApplicationArgs : IApplicationArgs
{
    public ApplicationArgs(string[] args) => Args = args;

    public string[] Args { get; }
}
