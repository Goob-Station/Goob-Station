using Content.Goobstation.Shared.HellGoose.Components;
using Content.Goobstation.Shared.HellGoose.Systems;
using Content.Server.Pointing.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.Interaction.Events;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.HellGoose;


public sealed class HellGooseStatueSystem : HellGooseStatueSharedSystem
{
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;

    protected override void PolymorphTheGoose(EntityUid uid)
    {
        base.PolymorphTheGoose(uid);
        _polymorphSystem.PolymorphEntity(uid, "HellGooseMorph");
    }

}