using System.CommandLine;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Owoify;


var outputOption = new Option<DirectoryInfo>(
    new[] { "--output", "-o" },
    description: "Destination of edited localization files.")
{
    IsRequired = true
};
var sourceOption = new Option<DirectoryInfo?>(
    new[] { "--source", "-s" },
    description: "Location of the unedited localisation files.");
var levelOption = new Option<Owoifier.OwoifyLevel>(
    new[] { "--level", "-l" },
    description: "Level of owoification.", getDefaultValue: () => Owoifier.OwoifyLevel.Owo);
var replaceOption = new Option<bool>(
    new[] { "--replace", "-r" },
    description: "Generate as a replace file.", getDefaultValue: () => false);

var rootCommand = new RootCommand("Owoify PowerShell localization");
rootCommand.AddOption(outputOption);
rootCommand.AddOption(sourceOption);
rootCommand.AddOption(levelOption);
rootCommand.AddOption(replaceOption);


rootCommand.SetHandler((output, source, level) =>
    {
        Regex regex = new Regex("(?<!resheader.+\\n.+)(?<=<value>)(.|\\n)*?(?=<\\/value>)", RegexOptions.Compiled);
        if (!output.Exists)
            Directory.CreateDirectory(output.FullName);
        source ??= output;

        Console.WriteLine($"Output = {output}");
        Console.WriteLine($"Source = {source}");
        Console.WriteLine($"Owoification level = {level}");

        var stopwatch = Stopwatch.StartNew();
        
        string[] paths =
        {
            @"/src/Microsoft.PowerShell.ConsoleHost/resources",
            @"/src/Microsoft.PowerShell.Commands.Utility/resources",
            @"/src/Microsoft.PowerShell.Commands.Management/resources",
            @"/src/Microsoft.PowerShell.Commands.Diagnostics/resources",
            @"/src/Microsoft.Management.UI.Internal/resources",
            @"/src/Microsoft.Management.Infrastructure.CimCmdlets/resources",
            @"/src/Microsoft.PowerShell.CoreCLR.Eventing/resources",
            @"/src/Microsoft.PowerShell.LocalAccounts/resources",
            @"/src/Microsoft.PowerShell.ScheduledJob/resources",
            @"/src/Microsoft.PowerShell.Security/resources",
            @"/src/Microsoft.WSMan.Management/resources",
            @"/src/System.Management.Automation/resources",
        };
        Parallel.ForEach(paths, path =>
        {
            path = Path.Join(source.FullName, path);
            foreach (var file in Directory.GetFiles(path))
            {
                var text = File.ReadAllText(file);
                var matches = regex.Matches(text);
                foreach (var match in matches)
                {
                    Console.WriteLine(((Match)match).Value);
                    var owo = Owoifier.Owoify(((Match)match).Value, level);
                    text = text.Replace(match!.ToString()!, owo);
                }

                File.WriteAllText(file.Replace(source.FullName, output.FullName), text);
            }
        });
        
        // var encoding = new UTF8Encoding(true);
        // Parallel.ForEach(Directory.GetFiles(source.FullName), file =>
        // {
        //     var lines = File.ReadAllLines(file);
        //     var str = new StringBuilder();
        //     str.AppendLine(lines[0]);
        //     for (var i = 1; i < lines.Length; i++)
        //     {
        //         var line = lines[i];
        //         if (string.IsNullOrWhiteSpace(line) || line[0] == '#' || line[1] == '#' || line[2] == '#' ||
        //             line[0] != ' ')
        //             continue;
        //
        //         var val = line[(line.IndexOf('"') + 1)..line.LastIndexOf('"')];
        //         var g = regex.Replace(val, match =>
        //         {
        //             if (match.ValueSpan[0] == '$' || match.ValueSpan[0] == '§' || match.ValueSpan[0] == '[' ||
        //                 match.ValueSpan[0] == '.' || match.ValueSpan[0] == '£')
        //                 return match.Value;
        //             return Owoifier.Owoify(match.Value, level);
        //         });
        //         var key = line.AsSpan()[1..line.IndexOf(' ', 2)];
        //         str.AppendLine($" {key} \"{g}\"");
        //     }
        //     
        //     var dest = Path.Join(output.FullName, Path.GetFileName(file));
        //     var fs = new FileStream(dest, FileMode.Create);
        //     var preamble = encoding.GetPreamble();
        //     fs.Write(preamble);
        //     fs.Write(encoding.GetBytes(str.ToString()));
        //     fs.Flush();
        //     fs.Close();
        // });
        Console.WriteLine($"Took {stopwatch.ElapsedMilliseconds}ms.");
    },
    outputOption, sourceOption, levelOption);

return await rootCommand.InvokeAsync(args);