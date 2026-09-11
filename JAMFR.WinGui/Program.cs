using JAMFR.Logic;
using JAMFR.Logic.Implementations;
using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

string? selectedPath = null;

var staThread = new Thread(() =>
{
    var dialog = new FolderBrowserDialog
    {
        Description = "Select library root or single album",
        UseDescriptionForTitle = true,
    };

    if (dialog.ShowDialog() == DialogResult.OK)
        selectedPath = dialog.SelectedPath;
});

staThread.SetApartmentState(ApartmentState.STA);
staThread.Start();
staThread.Join();

if (selectedPath is null || !Directory.Exists(selectedPath))
    throw new ArgumentNullException(nameof(selectedPath));

IAlbumFixer fixer = new AlbumFixer();
var dir = Directory.CreateDirectory(selectedPath);

var subdirs = dir.GetDirectories()
    .Where(d => d.GetFiles().Any(f => f.Name.EndsWith(".mp3")))
    .ToArray();

if (subdirs.Length == 0)
    await fixer.FixAlbumAsync(dir);
else
    await Task.WhenAll(subdirs.Select(fixer.FixAlbumAsync));