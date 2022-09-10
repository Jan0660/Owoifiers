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

var rootCommand = new RootCommand("Owoify Hearts of Iron 4 localization");
rootCommand.AddOption(outputOption);
rootCommand.AddOption(sourceOption);
rootCommand.AddOption(levelOption);


rootCommand.SetHandler((output, source, level) =>
    {
        var regex = new Regex(@"[^ ]\w+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        if (!output.Exists)
            Directory.CreateDirectory(output.FullName);
        source ??= output;

        Console.WriteLine($"Output = {output}");
        Console.WriteLine($"Source = {source}");
        Console.WriteLine($"Owoification level = {level}");

        var stopwatch = Stopwatch.StartNew();
        var encoding = new UTF8Encoding(true);
        Parallel.ForEach(Directory.GetFiles(source.FullName), file =>
        {
            var lines = File.ReadAllLines(file);
            var str = new StringBuilder();
            str.AppendLine(lines[0]);
            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line) || line[0] == '#' || line[1] == '#' || line[2] == '#' ||
                    line[0] != ' ')
                    continue;

                var val = line[(line.IndexOf('"') + 1)..line.LastIndexOf('"')];
                var g = regex.Replace(val, match =>
                {
                    if (match.ValueSpan[0] == '$' || match.ValueSpan[0] == '§' || match.ValueSpan[0] == '[' ||
                        match.ValueSpan[0] == '.' || match.ValueSpan[0] == '£')
                        return match.Value;
                    return Owoifier.Owoify(match.Value, level);
                });
                var key = line.AsSpan()[1..line.IndexOf(' ', 2)];
                str.AppendLine($" {key} \"{g}\"");
            }
            
            var dest = Path.Join(output.FullName, Path.GetFileName(file));
            var fs = new FileStream(dest, FileMode.Create);
            var preamble = encoding.GetPreamble();
            fs.Write(preamble);
            fs.Write(encoding.GetBytes(str.ToString()));
            fs.Flush();
            fs.Close();
        });
        Console.WriteLine($"Took {stopwatch.ElapsedMilliseconds}ms.");
    },
    outputOption, sourceOption, levelOption);

return await rootCommand.InvokeAsync(args);