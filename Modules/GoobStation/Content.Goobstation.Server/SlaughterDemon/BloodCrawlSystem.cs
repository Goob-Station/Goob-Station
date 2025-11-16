// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SlaughterDemon;
using Content.Goobstation.Shared.SlaughterDemon.Systems;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.Polymorph;
using Robust.Server.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.SlaughterDemon;

public sealed class BloodCrawlSystem : SharedBloodCrawlSystem
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

    protected override bool CheckAlreadyCrawling(Entity<BloodCrawlComponent> ent)
    {
        base.CheckAlreadyCrawling(ent);

        var component = ent.Comp;
        var uid = ent.Owner;

        if (!component.IsCrawling && _polymorphedQuery.TryComp(uid, out var polymorph))
        {
            var reverted = _polymorph.Revert(uid);

            if (reverted != null)
                _audio.PlayPvs(component.ExitJauntSound, reverted.Value);

            var evExit = new BloodCrawlExitEvent();
            RaiseLocalEvent(polymorph.Parent, ref evExit);

            return false;
        }
        return true;
    }

    protected override void PolymorphDemon(EntityUid user, ProtoId<PolymorphPrototype> polymorph)
    {
        base.PolymorphDemon(user, polymorph);

        _polymorph.PolymorphEntity(user, polymorph);
    }
}


