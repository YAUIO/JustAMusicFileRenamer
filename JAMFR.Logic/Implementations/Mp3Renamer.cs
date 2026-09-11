using System.Text;
using File = System.IO.File;

namespace JAMFR.Logic.Implementations;

public class Mp3Renamer : IFileRenamer
{
    private readonly IStringLocalizer _localizer = new StringLocalizer();
    
    public Task<string> RenameFileAsync(FileInfo file, int index)
    {
        if (!file.Name.EndsWith(".mp3"))
            return Task.FromResult(file.Name);

        try
        {
            using var mp3 = TagLib.File.Create(file.FullName);
            var tag = mp3.Tag;

            var artist = tag != null && tag.Performers.Length > 0
                ? TagTextRepair.FixMojibake(string.Join(",", tag.Performers))
                : null;
            var track = !string.IsNullOrEmpty(tag?.Title) ? TagTextRepair.FixMojibake(tag.Title) : null;

            var ind = tag?.Track > 0 ? (int)tag.Track : index + 1;

            if (ind > 99)
                throw new NotSupportedException("Index bigger than 99 is not supported yet");

            var formattedIndex = ind > 9 ? $"{ind}" : $"0{ind}";
            var nameBuilder = new StringBuilder($"{formattedIndex}");

            if (artist != null && (!track?.Contains(" - ") ?? false))
                nameBuilder.Append($"-{artist}");

            if (track != null)
                nameBuilder.Append($"-{track}");

            nameBuilder.Append(".mp3");

            var name = _localizer.Localize(nameBuilder.ToString());

            if (tag != null && (tag.Track != (uint)ind || tag.TitleSort != name))
            {
                mp3.Tag.Track = (uint)ind;
                mp3.Tag.TitleSort = name;
                mp3.Save();
            }

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
        catch (Exception e)
        {
            Console.WriteLine($"Couldn't rename the file: {e.Message}");
            return Task.FromResult(file.Name);
        }
    }
}