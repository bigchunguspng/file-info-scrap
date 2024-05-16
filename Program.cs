using System.Globalization;
using FileInfoScrap;
using static System.StringSplitOptions;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var invalidChars = Path.GetInvalidFileNameChars();

foreach (var arg in args)
{
    try
    {
        var report = ScanFileSystem(arg);

        var name = string.Join("-", arg.Split(invalidChars, RemoveEmptyEntries));
        var code = DateTime.UtcNow.Ticks % (1024 * 1024);
        var path = $"report-{code.ToString().PadLeft(7, '0')}-{name}.txt";
        using var stream = File.CreateText(path);
        foreach (var line in FormatReport(report))
        {
            stream.WriteLine(line);
            Console.WriteLine(line);
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Use this script with DIRECTORIES!");
        Console.ResetColor();
    }
}

Console.ReadKey();


IEnumerable<FileReport> ScanFileSystem(string path)
{
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine($"Scanning \"{path}\"");

    var directory = new DirectoryInfo(path);
    var files = directory.GetFiles("*", new EnumerationOptions
    {
        IgnoreInaccessible = true,
        RecurseSubdirectories = true,
        ReturnSpecialDirectories = false
    });

    Console.WriteLine($"Files found: {files.Length}\n");
    Console.ResetColor();

    return files.Select(x => new FileReport(x));
}

IEnumerable<string> FormatReport(IEnumerable<FileReport> files)
{
    const string f = "yyyy-MMM-dd' 'hh:mm:ss";
    const string s = "    ";

    yield return $"{"INDEX",8}{"FILE-SIZE",16}{s}{"CREATED",-20}{s}{"MODIFIED",-20}{s}NAME";

    var currentPath = "";
    var i = 0;
    foreach (var file in files)
    {
        i++;
        var path = Path.GetDirectoryName(file.Path);
        var name = Path.GetFileName     (file.Path);
        var kbs = MathF.Round(file.Length / 1024F, 2).ToString("N2");
        var ctd = file. CreationTimeUtc.ToLocalTime().ToString(f);
        var mod = file.LastWriteTimeUtc.ToLocalTime().ToString(f);

        if (string.Equals(currentPath, path) == false)
        {
            currentPath = path;
            yield return $"\n\n\t\t\t{s}{path}\n";
        }

        yield return $"{i,8}{kbs,16}{s}{ctd}{s}{mod}{s}{name}";
    }
}