namespace JAMFR.Logic.Implementations;

public class FatModificationSorter : IFatModificationSorter
{
    public Task SortAllAsync(FileInfo[] files)
    {
        var srcDir = files.First().DirectoryName!;
        var targetDir = Path.Combine(srcDir, "temp");

        Directory.CreateDirectory(targetDir);
        
        foreach (var file in files)
            file.MoveTo(Path.Combine(targetDir, file.Name));
        
        foreach (var file in files)
            file.MoveTo(Path.Combine(srcDir, file.Name));
        
        Directory.Delete(targetDir);
        
        var baseDate = new DateTime(2007, 1, 1);

        for (var i = 0; i < files.Length; i++)
        {
            var file = files[i];
            var stamp = baseDate.AddMinutes(i);

            File.SetCreationTime(file.FullName, stamp);
            File.SetLastWriteTime(file.FullName, stamp);
            File.SetLastAccessTime(file.FullName, stamp);
        }
        
        Console.WriteLine($"Sort order {string.Join(", ", files.Select(f => f.Name))}");

        return Task.CompletedTask;
    }
}