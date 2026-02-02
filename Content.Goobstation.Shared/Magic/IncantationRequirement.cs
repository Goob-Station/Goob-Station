using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Magic;

[ImplicitDataDefinitionForInheritors, Serializable, NetSerializable]
public abstract partial class IncantationRequirement
{
    public abstract bool Valid(EntityUid ent, IEntityManager entMan, out string reason);
}
