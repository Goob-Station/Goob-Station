// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
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

    private void OnInit(EntityUid uid, DeathSquadMemberComponent comp, ref MapInitEvent args)
    {
        var originalCrit = _threshold.GetThresholdForState(uid, MobState.Critical);
        var originalDead = _threshold.GetThresholdForState(uid, MobState.Dead);

        var newCrit = originalCrit + comp.NewHealth;
        var newDead = originalDead + comp.NewHealth;

        _threshold.SetMobStateThreshold(uid, newCrit, MobState.Critical);
        _threshold.SetMobStateThreshold(uid, newDead, MobState.Dead);
    }

    private void OnExamined(EntityUid uid, DeathSquadMemberComponent comp, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var details = Loc.GetString("death-squad-examined", ("target", Identity.Entity(uid, EntityManager)));
        args.PushMarkup(details, 5);
    }
}
