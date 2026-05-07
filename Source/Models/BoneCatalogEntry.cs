namespace BoneSmith.Models;

public sealed class BoneCatalogEntry
{
    public string Name { get; init; } = string.Empty;
    public BoneCategory Category { get; init; } = BoneCategory.Unknown;
    public int PartialSkeletonIndex { get; init; }
    public int BoneIndex { get; init; }
}