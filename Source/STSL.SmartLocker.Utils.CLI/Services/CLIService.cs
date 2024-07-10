using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using STSL.SmartLocker.Utils.CLI.Models;
using STSL.SmartLocker.Utils.CLI.Services.Contracts;
using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace STSL.SmartLocker.Utils.CLI.Services;

internal sealed class CLIService : IHostedService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILockerService _lockerService;
    private readonly string[] _args;
    private readonly RootCommand _rootCommand = new("Temporary Locker Config CLI");
    private readonly JsonSerializerOptions _jsonConfig = new() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    public CLIService(
            ILogger<CLIService> logger,
            IHostApplicationLifetime appLifetime,
            ILockerService lockerService,
            IApplicationArgs applicationArgs)
    {
        _logger = logger;
        _appLifetime = appLifetime;
        _lockerService = lockerService;
        _args = applicationArgs.Args;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifetime.ApplicationStarted.Register(() =>
        {
            Task.Run(async () =>
            {
                try
                {
                    SetupCommands();

                    Environment.ExitCode = await _rootCommand.InvokeAsync(_args);
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception!");
                }
                finally
                {
                    _appLifetime.StopApplication();
                }
            });
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void SetupCommands()
    {
        var simpleConfigOption = new Option<bool>("--simple", "Use abstracted config");
        simpleConfigOption.AddAlias("-s");

        var lockerConfigFileOption = new Option<FileInfo>(
        name: "--config-file",
        description: "The locker's config in JSON format",
        parseArgument: option =>
        {
            var filePath = option.Tokens.Single().Value;
            return File.Exists(filePath) ? new FileInfo(filePath) : throw new ArgumentException($"File {filePath} doesn't exist!", nameof(filePath));
        });
        lockerConfigFileOption.AddAlias("-f");

        var lockerConfigSetupManyOption = new Option<bool>(
                name: "--many",
                description: "Setup many lockers at once"
            );
        lockerConfigSetupManyOption.AddAlias("-m");

        var lockerIdArgument = new Argument<LockerSerial>("lockerId", "The locker's unique serial");

        SetupGetConfigCommand(lockerIdArgument, simpleConfigOption);
        SetupPutConfigCommand(lockerConfigFileOption, lockerConfigSetupManyOption);
        SetupPatchConfigCommand(lockerConfigFileOption, lockerConfigSetupManyOption);
        SetupResetConfigCommand(lockerIdArgument);
    }

    private void SetupResetConfigCommand(Argument<LockerSerial> lockerIdArgument)
    {
        var resetConfigCommand = new Command("reset", "Reset locker config")
        {
            lockerIdArgument
        };

        resetConfigCommand.SetHandler(async lockerId =>
        {
            var lockerConfigJson = new LockerConfigJson
            {
                Property = Property.WithDefaults()
            };

            await JsonSerializer.SerializeAsync(Console.OpenStandardOutput(), lockerConfigJson, _jsonConfig);
            Console.WriteLine();
            await _lockerService.SetLockerConfig(lockerConfigJson, lockerId);
        }, lockerIdArgument);

        _rootCommand.AddCommand(resetConfigCommand);
    }

    public void SetupPatchConfigCommand(Option<FileInfo> lockerConfigFileOption, Option<bool> lockerConfigSetupManyOption)
    {
        var patchConfigCommand = new Command("patch", "Patch / partial update locker config")
        {
            lockerConfigFileOption,
            lockerConfigSetupManyOption
        };
        patchConfigCommand.SetHandler(async (configFile, useMany) =>
        {
            var fileText = await File.ReadAllTextAsync(configFile.FullName);
            if (useMany)
            {
                var config = JsonSerializer.Deserialize<LockersSetup>(fileText);

                if (config is not null)
                {
                    foreach (var locker in config.Lockers)
                    {
                        var convertedLocker = LockerConfig.ConvertToEndpointFormat(locker);
                        await JsonSerializer.SerializeAsync(Console.OpenStandardOutput(), convertedLocker, _jsonConfig);
                        Console.WriteLine();
                    }
                    await _lockerService.SetupLockers(config);
                }
            }
            else
            {
                var config = JsonSerializer.Deserialize<LockerConfig>(fileText);

                if (config is not null)
                {
                    await JsonSerializer.SerializeAsync(Console.OpenStandardOutput(), LockerConfig.ConvertToEndpointFormat(config), _jsonConfig);
                    await _lockerService.SetLockerConfig(config);
                }
            }
        }, lockerConfigFileOption, lockerConfigSetupManyOption);
        _rootCommand.AddCommand(patchConfigCommand);
    }

    public void SetupPutConfigCommand(Option<FileInfo> lockerConfigFileOption, Option<bool> lockerConfigSetupManyOption)
    {

        var putConfigCommand = new Command("put", "Put locker config")
        {
            lockerConfigFileOption,
            lockerConfigSetupManyOption
        };
        putConfigCommand.SetHandler(async (configFile, useMany) =>
        {
            var fileText = await File.ReadAllTextAsync(configFile.FullName);
            if (useMany)
            {
                var config = JsonSerializer.Deserialize<LockersSetup>(fileText);

                if (config is not null)
                {
                    foreach (var locker in config.Lockers)
                    {
                        var convertedConfig = LockerConfig.ConvertToEndpointFormat(locker);

                        var lockerConfigJson = new LockerConfigJson
                        {
                            Property = Property.OverwriteWith(Property.WithDefaults(), convertedConfig.Property)
                        };

                        await JsonSerializer.SerializeAsync(Console.OpenStandardOutput(), lockerConfigJson, _jsonConfig);
                        Console.WriteLine();
                        await _lockerService.SetLockerConfig(lockerConfigJson, locker.Id);
                    }
                }
            }
            else
            {
                var config = JsonSerializer.Deserialize<LockerConfig>(fileText);

                if (config is not null)
                {
                    var convertedConfig = LockerConfig.ConvertToEndpointFormat(config);

                    var lockerConfigJson = new LockerConfigJson
                    {
                        Property = Property.OverwriteWith(Property.WithDefaults(), convertedConfig.Property)
                    };

                    await JsonSerializer.SerializeAsync(Console.OpenStandardOutput(), lockerConfigJson, _jsonConfig);
                    await _lockerService.SetLockerConfig(lockerConfigJson, config.Id);
                }
            }
        }, lockerConfigFileOption, lockerConfigSetupManyOption);

        _rootCommand.AddCommand(putConfigCommand);
    }

    public void SetupGetConfigCommand(Argument<LockerSerial> lockerIdArgument, Option<bool> simpleConfigOption)
    {
        var getConfigCommand = new Command("get", "Get locker config")
        {
            simpleConfigOption,
            lockerIdArgument
        };
        getConfigCommand.SetHandler(async (lockerId, useSimpleConfig) =>
        {
            if (useSimpleConfig)
            {
                var config = await _lockerService.GetLockerConfig(lockerId);
                await JsonSerializer.SerializeAsync(Console.OpenStandardOutput(), config, _jsonConfig);
            }
            else
            {
                var config = await _lockerService.GetLockerConfigJson(lockerId);
                await JsonSerializer.SerializeAsync(Console.OpenStandardOutput(), config, _jsonConfig);
            }

        }, lockerIdArgument, simpleConfigOption);

        _rootCommand.AddCommand(getConfigCommand);
    }
}