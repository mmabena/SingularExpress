# Options

These are typed settings classes, one per `appsettings` section. They are validated at start-up in `Extensions/OptionsExtensions.cs`.

To add a setting: add the property (with a validation attribute), add it to `appsettings.example.json` with a safe value, and inject `IOptions<XOptions>`. **Secrets never go in appsettings files.**
