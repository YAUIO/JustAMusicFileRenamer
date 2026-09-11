using System.Data;

namespace JAMFR.Logic.Implementations;

public class AlbumFixer : IAlbumFixer
{
    private readonly IFileRenamer _renamer = new Mp3Renamer();
    private readonly IStringLocalizer _localizer = new StringLocalizer();
    private readonly IFatModificationSorter _sorter = new FatModificationSorter();
    
    public async Task FixAlbumAsync(DirectoryInfo dir)
    {
        var origName = dir.Name;
        var normalizedName = _localizer.Localize(dir.Name);

        var path = dir.FullName.Replace(origName, normalizedName);
        
        if (dir.FullName != path)
            Directory.Move(dir.FullName, path);

        if (!Directory.Exists(path))
            throw new ConstraintException($"Directory at path {path} does not exist");

        var newDir = Directory.CreateDirectory(path);
        var files = newDir.GetFiles()
            .Where(f => f.Name.EndsWith(".mp3"))
            .ToArray();
        
        Array.Sort(files, (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        
        await Task.WhenAll(files.Select(f => _renamer.RenameFileAsync(f, files.IndexOf(f))));
        
        Array.Sort(files, (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

        await _sorter.SortAllAsync(files);
    }
}