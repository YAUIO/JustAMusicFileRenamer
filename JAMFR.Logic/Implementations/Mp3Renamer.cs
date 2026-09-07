using System.Text;
using Id3;

namespace JAMFR.Logic.Implementations;

public class Mp3Renamer : IFileRenamer
{
    private readonly IStringLocalizer _localizer = new StringLocalizer();
    
    public Task<string> RenameFileAsync(FileInfo file, int index)
    {
        if (!file.Name.EndsWith(".mp3"))
            return Task.FromResult(file.Name);

        Id3Tag? tag;
        try
        {
            using var mp3 = new Mp3(file.FullName);
            tag = mp3.GetTag(Id3TagFamily.Version2X);

            var artistsAssigned = tag?.Artists is { IsAssigned: true };
            if (!artistsAssigned)
            {
                var v1Tag = mp3.GetTag(Id3TagFamily.Version1X);
                if (v1Tag != null)
                    tag = v1Tag;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to read ID3 tag for {file.FullName}: {ex.Message}");
            tag = null;
        }

        var artist = tag != null ? TagTextRepair.FixMojibake(tag.Artists?.ToString()) : null;
        var track = tag != null ? TagTextRepair.FixMojibake(tag.Title) : null;

        var ind = tag?.Track.Value > 0 ? tag.Track.Value : index;

        var formattedIndex = ind >= 9 ? $"{ind + 1}" : $"0{ind + 1}";
        var nameBuilder = new StringBuilder($"({formattedIndex})");

        if (artist != null)
            nameBuilder.Append($"-{artist}");

        if (track != null)
            nameBuilder.Append($"-{track}");

        nameBuilder.Append(".mp3");

        var name = _localizer.Localize(nameBuilder.ToString());

        if (file.DirectoryName == null)
            throw new ArgumentNullException(file.DirectoryName, $"Directory name was null for track {file.Name}");

        var newPath = Path.Combine(file.DirectoryName, name);
        
        if (newPath != file.FullName)
        {
            Console.WriteLine($"From {file.FullName} to {newPath}");
            File.Move(file.FullName, newPath);
        }
        else
        {
            Console.WriteLine($"Skipped: {file.FullName}");
        }

        return Task.FromResult(name);
    }
}