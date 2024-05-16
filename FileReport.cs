namespace FileInfoScrap;

internal class FileReport
{
    public FileReport(FileInfo file)
    {
        Path = file.FullName;
        Length = file.Length;
        CreationTimeUtc = file.CreationTimeUtc;
        LastWriteTimeUtc = file.LastWriteTimeUtc;
    }

    public readonly string Path;
    public readonly long Length;
    public DateTime CreationTimeUtc;
    public DateTime LastWriteTimeUtc;
}