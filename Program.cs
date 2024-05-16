using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static System.StringSplitOptions;

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
        using var writer = new JsonTextWriter(stream);
        GetSerializer().Serialize(writer, report);
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
    Console.WriteLine($"\tScanning: [{path}]");

    var directory = new DirectoryInfo(path);
    var files = directory.GetFiles("*", new EnumerationOptions
    {
        IgnoreInaccessible = true,
        RecurseSubdirectories = true,
        ReturnSpecialDirectories = false
    });

    Console.WriteLine($"\tFiles found: [{files.Length}]");

    return files.Select(x => new FileReport(x));
}

JsonSerializer GetSerializer() => new()
{
    Formatting = Formatting.Indented,
    ContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new KebabCaseNamingStrategy()
    },
    DefaultValueHandling = DefaultValueHandling.Populate
};

internal class FileReport
{
    public FileReport(FileInfo file)
    {
        Path = file.FullName;
        Length = file.Length;
        CreationTimeUtc = file.CreationTimeUtc;
        LastAccessTimeUtc = file.LastAccessTimeUtc;
        LastWriteTimeUtc = file.LastWriteTimeUtc;
    }

    [JsonProperty] public string Path;
    [JsonProperty] public long Length;
    [JsonProperty] public DateTime CreationTimeUtc;
    [JsonProperty] public DateTime LastAccessTimeUtc;
    [JsonProperty] public DateTime LastWriteTimeUtc;
}