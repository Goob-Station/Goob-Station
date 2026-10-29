using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Minions.Plaguebringer;
using Content.Server.Polymorph.Systems;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Wraith.Systems.Minions;

public sealed partial class DiseasedRatSystem : SharedDiseasedRatSystem
{
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private PolymorphSystem _polymorph = default!;

    protected override void Evolve(EntityUid uid, ProtoId<DiseasedRatFormUnlockPrototype> newProto)
    {
        base.Evolve(uid, newProto);

        if (!_proto.TryIndex(newProto, out var index)
            || index.Entity == null
            || index.TransferComponents == null)
            return;

        var poly = new PolymorphConfiguration
        {
            Entity = index.Entity,
            TransferName = true,
            TransferDamage = true,
            Forced = true,
            RevertOnCrit = false,
            RevertOnDeath = false,
            ComponentsToTransfer = index.TransferComponents,
            AllowRepeatedMorphs = true,
        };

        _polymorph.PolymorphEntity(uid, poly);
    }
}
