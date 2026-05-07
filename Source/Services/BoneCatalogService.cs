using BoneSmith.Models;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

namespace BoneSmith.Services;

public unsafe sealed class BoneCatalogService
{
    private readonly IObjectTable objectTable;
    private readonly IPluginLog log;

    public List<BoneCatalogEntry> LastCatalog { get; private set; } = [];

    public BoneCatalogService(IObjectTable objectTable, IPluginLog log)
    {
        this.objectTable = objectTable;
        this.log = log;
    }

    public IReadOnlyList<BoneCatalogEntry> Refresh()
    {
        var result = new List<BoneCatalogEntry>();

        try
        {
            var localPlayer = objectTable[0];

            if (localPlayer is null || localPlayer.Address == nint.Zero)
            {
                LastCatalog = result;
                return LastCatalog;
            }

            var gameObject = (GameObject*)localPlayer.Address;
            var cBase = (CharacterBase*)gameObject->DrawObject;

            if (cBase == null || cBase->Skeleton == null)
            {
                LastCatalog = result;
                return LastCatalog;
            }

            for (var partialIndex = 0; partialIndex < cBase->Skeleton->PartialSkeletonCount; partialIndex++)
            {
                var partial = cBase->Skeleton->PartialSkeletons[partialIndex];
                var pose = partial.GetHavokPose(0);

                if (pose == null || pose->Skeleton == null)
                    continue;

                var boneCount = pose->Skeleton->Bones.Length;

                for (var boneIndex = 0; boneIndex < boneCount; boneIndex++)
                {
                    var name = pose->Skeleton->Bones[boneIndex].Name.String;

                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    result.Add(new BoneCatalogEntry
                    {
                        Name = name,
                        Category = Categorize(name),
                        PartialSkeletonIndex = partialIndex,
                        BoneIndex = boneIndex,
                    });
                }
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, "BoneSmith failed to refresh bone catalog.");
        }

        LastCatalog = result
            .DistinctBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return LastCatalog;
    }

    private static BoneCategory Categorize(string boneName)
    {
        var n = boneName.ToLowerInvariant();

        if (n.Contains("root"))
            return BoneCategory.Root;

        if (n.Contains("spine") || n.Contains("chest") || n.Contains("waist") || n.Contains("hip") || n.Contains("clavicle"))
            return BoneCategory.SpineTorso;

        if (n.Contains("head") || n.Contains("neck") || n.Contains("face") || n.Contains("jaw") || n.Contains("eye") || n.Contains("brow") || n.Contains("mouth") || n.Contains("lip"))
            return BoneCategory.HeadFace;

        if (n.Contains("arm") || n.Contains("shoulder") || n.Contains("elbow") || n.Contains("forearm"))
            return BoneCategory.Arms;

        if (n.Contains("hand") || n.Contains("finger") || n.Contains("thumb"))
            return BoneCategory.HandsFingers;

        if (n.Contains("leg") || n.Contains("thigh") || n.Contains("knee") || n.Contains("calf"))
            return BoneCategory.Legs;

        if (n.Contains("foot") || n.Contains("toe") || n.Contains("ankle"))
            return BoneCategory.Feet;

        if (n.Contains("tail"))
            return BoneCategory.Tail;

        if (n.Contains("ear") || n.Contains("hair") || n.Contains("skl_") || n.Contains("accessory"))
            return BoneCategory.EarsHairAccessories;

        if (n.Contains("weapon") || n.Contains("prop") || n.Contains("attach"))
            return BoneCategory.WeaponsProps;

        return BoneCategory.Unknown;
    }
}