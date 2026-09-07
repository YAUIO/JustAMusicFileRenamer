namespace JAMFR.Logic;

public interface IFileRenamer
{
    Task<string> RenameFileAsync(FileInfo path, int index);
}