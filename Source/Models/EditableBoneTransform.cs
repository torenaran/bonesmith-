using System.Numerics;

namespace BoneSmith.Models;

public sealed class EditableBoneTransform
{
    public bool Enabled { get; set; }

    public Vector3 Translation { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public Vector3 Scaling { get; set; } = Vector3.One;
    public Vector3 ChildScaling { get; set; } = Vector3.One;

    public bool PropagateTranslation { get; set; }
    public bool PropagateRotation { get; set; }
    public bool PropagateScale { get; set; }
    public bool ChildScalingIndependent { get; set; }

    public bool IsEdited =>
        Enabled &&
        (!Approximately(Translation, Vector3.Zero) ||
         !Approximately(Rotation, Vector3.Zero) ||
         !Approximately(Scaling, Vector3.One) ||
         !Approximately(ChildScaling, Vector3.One) ||
         PropagateTranslation ||
         PropagateRotation ||
         PropagateScale ||
         ChildScalingIndependent);

    private static bool Approximately(Vector3 a, Vector3 b)
    {
        const float epsilon = 0.00001f;

        return MathF.Abs(a.X - b.X) < epsilon &&
               MathF.Abs(a.Y - b.Y) < epsilon &&
               MathF.Abs(a.Z - b.Z) < epsilon;
    }
}