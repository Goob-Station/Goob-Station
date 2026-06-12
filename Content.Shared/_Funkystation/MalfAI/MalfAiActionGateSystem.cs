// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions.Events;
using Content.Shared.Silicons.StationAi;

namespace Content.Shared._Funkystation.MalfAI;

/// <summary>
/// Cancels Malf AI ability use when the AI is not held (e.g. downloaded into an intellicard).
/// </summary>
public sealed class MalfAiActionGateSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiActionComponent, ActionAttemptEvent>(OnActionAttempt);
    }

    private void OnActionAttempt(Entity<MalfAiActionComponent> ent, ref ActionAttemptEvent args)
    {
        // StationAiHeld is granted by the AiHeld registry: present in the core and in a
        // shunted APC, absent inside an intellicard. A brain piloting a hijacked mech
        // also lacks it but still needs its Return to Core action.
        if (!HasComp<StationAiHeldComponent>(args.User) && !HasComp<MalfAiMechHijackComponent>(args.User))
            args.Cancelled = true;
    }
}
