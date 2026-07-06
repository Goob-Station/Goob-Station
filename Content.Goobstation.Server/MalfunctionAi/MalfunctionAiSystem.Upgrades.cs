// SPDX-FileCopyrightText: 2026 Jonikibaka
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Light.Components;
using Content.Server.Mind;
using Content.Server.Pinpointer;
using Content.Server.Power.Components;
using Content.Shared.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Radio.Components;
using Content.Server.Silicons.Laws;
using Content.Server.Station.Systems;
using Content.Server.Store.Systems;
using Content.Shared.SurveillanceCamera.Components;
using Content.Server.VoiceMask;
using Content.Goobstation.Shared.MalfunctionAi;
using Content.Goobstation.Shared.Overlays;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Chat.RadioIconsEvents;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Content.Shared.VoiceMask;
using Robust.Shared.Player;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Electrocution;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.StationAi;
using Content.Shared.Turrets;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Popups;
using Content.Shared.RCD.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Content.Shared.Verbs;
using System.Numerics;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.MalfunctionAi;

public sealed partial class MalfunctionAiSystem
{
    // --- AI turret upgrade ---

    private void OnTurretUpgrade(Entity<MalfunctionAiComponent> ent, ref MalfTurretUpgradeEvent args)
    {
        // Presence of the component marks the upgrade as bought.
        var upgrade = EnsureComp<MalfTurretUpgradeComponent>(ent.Owner);

        var count = 0;
        var query = EntityQueryEnumerator<StationAiTurretComponent>();
        while (query.MoveNext(out var turretUid, out _))
        {
            ApplyTurretUpgrade(turretUid, upgrade);
            count++;
        }

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-turrets-success", ("count", count)), ent.Owner);
    }

    private void ApplyTurretUpgrade(EntityUid turret, MalfTurretUpgradeComponent upgrade)
    {
        // Power: the turret shoots faster.
        if (TryComp<GunComponent>(turret, out var gun))
        {
            gun.FireRate *= upgrade.FireRateMultiplier;
            Dirty(turret, gun);
            _gun.RefreshModifiers((turret, gun));
        }

        // Durability: no longer fragile while its cover is open.
        if (TryComp<DeployableTurretComponent>(turret, out var deployable))
            _turrets.SetResilientWhenDeployed((turret, deployable));
    }

    private void OnTurretMapInit(Entity<StationAiTurretComponent> ent, ref MapInitEvent args)
    {
        // Newly built AI turrets inherit the upgrade.
        var query = EntityQueryEnumerator<MalfTurretUpgradeComponent>();
        if (query.MoveNext(out _, out var upgrade))
            ApplyTurretUpgrade(ent.Owner, upgrade);
    }
}
