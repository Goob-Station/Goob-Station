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

    private void OnOpenStore(Entity<MalfunctionAiComponent> ent, ref MalfOpenStoreEvent args)
    {
        if (args.Handled)
            return;

        _store.ToggleUi(ent.Owner, ent.Owner);
        args.Handled = true;
    }
}
