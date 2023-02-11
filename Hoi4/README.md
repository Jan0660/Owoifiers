# Hoi4 Owo

Made using .NET and [deadshot465/Owoify.Net](https://github.com/deadshot465/Owoify.Net).

[Mods collection](https://steamcommunity.com/sharedfiles/filedetails/?edit=true&id=2861060419)

## Usage

```pwsh
pwsh> dotnet run -- --help
Description:
  Owoify Hearts of Iron 4 localization

Usage:
  Hoi4 [options]

Options:
  -o, --output <output> (REQUIRED)  Destination of edited localization files.
  -s, --source <source>             Location of the unedited localisation files.
  -l, --level <Owo|Uvu|Uwu>         Level of owoification. [default: Owo]
  --version                         Show version information
  -?, -h, --help                    Show help and usage information
```

If no source is specified then the output is used as source.
For the levels, Owo is the weakest, and Uvu is the strongest.
