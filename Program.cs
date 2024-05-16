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
        var path = $@"report-{code.ToString().PadLeft(7, '0')}-{name}.txt";
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

    yield return $"   INDEX   FILE-SIZE\t{"CREATED",-20}\t{"MODIFIED",-20}\tNAME";

    var currentPath = "";
    var i = 0;
    foreach (var file in files)
    {
        i++;
        var path = Path.GetDirectoryName(file.Path);
        var name = Path.GetFileName     (file.Path);
        var kb = MathF.Round(file.Length / 1024F, 2).ToString("N2");
        var c = file. CreationTimeUtc.ToLocalTime().ToString(f);
        var m = file.LastWriteTimeUtc.ToLocalTime().ToString(f);

        var xd = string.Equals(currentPath, path) == false;
        if (xd)
        {
            currentPath = path;
            yield return $"\n\n\t\t\t{path}\n";
        }

        yield return $"{i,8}{kb,12}\t{c}\t{m}\t{name}";
    }
}