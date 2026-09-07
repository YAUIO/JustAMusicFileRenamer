namespace JAMFR.Logic;

public interface IAlbumFixer
{
    Task FixAlbumAsync(DirectoryInfo dir);
}