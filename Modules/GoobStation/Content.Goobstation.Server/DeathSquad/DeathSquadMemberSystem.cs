// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Goobstation.Server.DeathSquad;

/// <summary>
/// In the future, I want this to block martial arts.
/// </summary>
public sealed partial class DeathSquadMemberSystem : EntitySystem
{
    [Dependency] private readonly MobThresholdSystem _threshold = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeathSquadMemberComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<DeathSquadMemberComponent, MapInitEvent>(OnInit);
    }

    private void OnInit(Entity<DeathSquadMemberComponent> deathSquad, ref MapInitEvent args)
    {
        if (!HasComp<MobStateComponent>(deathSquad))
            return;

        var originalCrit = _threshold.GetThresholdForState(deathSquad, MobState.Critical);
        var originalDead = _threshold.GetThresholdForState(deathSquad, MobState.Dead);

        var newCrit = originalCrit + deathSquad.Comp.NewHealth;
        var newDead = originalDead + deathSquad.Comp.NewHealth;

        _threshold.SetMobStateThreshold(deathSquad, newCrit, MobState.Critical);
        _threshold.SetMobStateThreshold(deathSquad, newDead, MobState.Dead);
    }

    private void OnExamined(Entity<DeathSquadMemberComponent> deathSquad, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var details = Loc.GetString("death-squad-examined", ("target", Identity.Entity(deathSquad, EntityManager)));
        args.PushMarkup(details, 5);
    }
}
