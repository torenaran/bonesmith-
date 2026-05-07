namespace BoneSmith.Models;

public sealed class BoneSmithTemplate
{
    public string UniqueId { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "New BoneSmith Template";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

    public Dictionary<string, EditableBoneTransform> Bones { get; set; }
        = new(StringComparer.OrdinalIgnoreCase);
}