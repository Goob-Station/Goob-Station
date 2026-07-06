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

    // --- Hack cyborg ---

    private void OnHackCyborg(Entity<MalfunctionAiComponent> ent, ref MalfHackCyborgEvent args)
    {
        if (args.Handled)
            return;

        // Prefer the cyborg actually under the cursor.
        if (args.Entity is { } hovered
            && HasComp<BorgChassisComponent>(hovered)
            && TryHackCyborg(ent, hovered))
        {
            args.Handled = true;
            return;
        }

        foreach (var candidate in _lookup.GetEntitiesInRange(args.Target, 0.75f))
        {
            if (!HasComp<BorgChassisComponent>(candidate))
                continue;

            if (TryHackCyborg(ent, candidate))
            {
                args.Handled = true;
                return;
            }
        }

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-invalid-cyborg"), ent.Owner);
    }

    private bool TryHackCyborg(Entity<MalfunctionAiComponent> ent, EntityUid target)
    {
        if (!HasComp<BorgChassisComponent>(target))
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-invalid-cyborg"), ent.Owner);
            return false;
        }

        if (HasComp<MalfHackedCyborgComponent>(target))
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-cyborg-already-hacked"), ent.Owner);
            return false;
        }

        // Keep the borg's normal laws but prepend the hidden malfunction law 0, flagging it as an antag.
        // The borg player hears the same malf theme the AI got with its briefing.
        var subvertSound = CompOrNull<MalfHackCyborgComponent>(ent.Owner)?.SubvertSound;
        if (!_law.AddMalfunctionLaw(target, ensureSubvertedRole: true, cue: subvertSound))
        {
            // Already subverted (e.g. emagged).
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-cyborg-already-hacked"), ent.Owner);
            return false;
        }

        AddComp<MalfHackedCyborgComponent>(target);
        ent.Comp.HackedCyborgs.Add(target);
        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-hack-cyborg-success"), ent.Owner);
        return true;
    }

    // --- Subverted borgs window ---

    private void OnOpenBorgsUi(Entity<MalfunctionAiComponent> ent, ref MalfOpenBorgsUiEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<ActorComponent>(ent.Owner, out var actor))
            return;

        _ui.OpenUi(ent.Owner, MalfBorgsUiKey.Key, actor.PlayerSession);
        UpdateBorgsUi(ent);
        args.Handled = true;
    }

    private void UpdateBorgsUi(Entity<MalfunctionAiComponent> ent)
    {
        if (!_ui.IsUiOpen(ent.Owner, MalfBorgsUiKey.Key))
            return;

        var entries = new List<MalfBorgEntry>();
        foreach (var borg in ent.Comp.HackedCyborgs)
        {
            if (!Exists(borg) || Deleted(borg))
                continue;

            entries.Add(new MalfBorgEntry
            {
                Name = Name(borg),
                Alive = _mobState.IsAlive(borg),
                Location = _navMap.GetNearestBeaconString((borg, Transform(borg))),
            });
        }

        _ui.SetUiState(ent.Owner, MalfBorgsUiKey.Key, new MalfBorgsBuiState(entries));
    }
}
