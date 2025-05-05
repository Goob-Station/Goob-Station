// SPDX-FileCopyrightText: 2025 AftrLite <61218133+AftrLite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Bible; // Goobstation - Bible
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult.Components.Examine;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicLapseSystem : EntitySystem
{
    [Dependency] private readonly CosmicCultSystem _cult = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    private static readonly ProtoId<PolymorphPrototype> HumanLapse = "CosmicLapseMobHuman";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicCultComponent, EventCosmicLapse>(OnCosmicLapse);
    }

    private void OnCosmicLapse(Entity<CosmicCultComponent> uid, ref EventCosmicLapse action)
    {
        if (action.Handled
            || HasComp<CosmicBlankComponent>(action.Target)
            || HasComp<CleanseCultComponent>(action.Target)
            || HasComp<BibleUserComponent>(action.Target))
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-generic-fail"), uid, uid);
            return;
        }

        action.Handled = true;
        var tgtpos = Transform(action.Target).Coordinates;
        Spawn(uid.Comp.LapseVFX, tgtpos);
        _popup.PopupEntity(Loc.GetString("cosmicability-lapse-success",
            ("target", Identity.Entity(action.Target, EntityManager))),
            uid,
            uid);
        var species = Comp<HumanoidAppearanceComponent>(action.Target).Species;
        var polymorphId = "CosmicLapseMob" + species;

        if (_prototype.HasIndex<PolymorphPrototype>(polymorphId))
            _polymorph.PolymorphEntity(action.Target, polymorphId);
        else
            _polymorph.PolymorphEntity(action.Target, HumanLapse);

        _cult.MalignEcho(uid);
    }
}
