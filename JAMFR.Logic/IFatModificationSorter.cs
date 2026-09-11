namespace JAMFR.Logic;

public interface IFatModificationSorter
{
    Task SortAllAsync(FileInfo[] files);
}