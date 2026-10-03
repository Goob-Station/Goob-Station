// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SlaughterDemon;
using Content.Goobstation.Shared.SlaughterDemon.Systems;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.Polymorph;
using Robust.Server.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.SlaughterDemon;

public sealed class ReagentCrawlSystem : SharedReagentCrawlSystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    private EntityQuery<PolymorphedEntityComponent> _polymorphedQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _polymorphedQuery = GetEntityQuery<PolymorphedEntityComponent>();
    }

    protected override bool CheckAlreadyCrawling(Entity<ReagentCrawlComponent> ent)
    {
        base.CheckAlreadyCrawling(ent);

        var component = ent.Comp;
        var uid = ent.Owner;

        if (component.IsCrawling || !_polymorphedQuery.TryComp(uid, out var polymorph))
            return true;
        var reverted = _polymorph.Revert(uid);

        if (reverted != null)
            _audio.PlayPvs(component.ExitJauntSound, reverted.Value);

        if (polymorph.Parent is not { } parent)
            return false;

        var evExit = new ReagentCrawlExitEvent();
        RaiseLocalEvent(parent, ref evExit);

        return false;
    }

    protected override void PolymorphEntity(EntityUid user, ProtoId<PolymorphPrototype> polymorph)
    {
        base.PolymorphEntity(user, polymorph);

        _polymorph.PolymorphEntity(user, polymorph);
    }
}


