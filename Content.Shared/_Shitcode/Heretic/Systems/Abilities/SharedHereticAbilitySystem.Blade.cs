// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BramvanZijp <56019239+BramvanZijp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Slippery;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    [Dependency] private readonly SharedStaminaSystem _stam = default!;

    protected virtual void SubscribeBlade()
    {
        // Protective blades prevent that
        // SubscribeLocalEvent<SilverMaelstromComponent, BeforeStaminaDamageEvent>(OnBeforeBladeStaminaDamage);
        // Still knocked down by a flashbang or something - it's fine
        // SubscribeLocalEvent<SilverMaelstromComponent, BeforeStatusEffectAddedEvent>(OnBeforeBladeStatusEffect);
        // Remove this after adding noslip heretic magboots side knowledge
        SubscribeLocalEvent<SilverMaelstromComponent, SlipAttemptEvent>(OnBladeSlipAttempt);
        // Protective blades do that
        // SubscribeLocalEvent<SilverMaelstromComponent, BeforeHarmfulActionEvent>(OnBladeHarmfulAction);

        SubscribeLocalEvent<RealignmentComponent, BeforeStaminaDamageEvent>(OnBeforeBladeStaminaDamage);
        SubscribeLocalEvent<RealignmentComponent, BeforeStatusEffectAddedEvent>(OnBeforeBladeStatusEffect);
        SubscribeLocalEvent<RealignmentComponent, SlipAttemptEvent>(OnBladeSlipAttempt);
        SubscribeLocalEvent<RealignmentComponent, BeforeHarmfulActionEvent>(OnBladeHarmfulAction);
        SubscribeLocalEvent<RealignmentComponent, StatusEffectEndedEvent>(OnStatusEnded);
        SubscribeLocalEvent<RealignmentComponent, ComponentRemove>(OnComponentRemove);
    }

    private void OnComponentRemove(Entity<RealignmentComponent> ent, ref ComponentRemove args) =>
        _stam.ToggleStaminaDrain(ent, 0, false, true, ent.Comp.StaminaRegenKey);

    private void OnStatusEnded(Entity<RealignmentComponent> ent, ref StatusEffectEndedEvent args)
    {
        if (args.Key != "Pacified")
            return;

        if (!_status.TryRemoveStatusEffect(ent, "Realignment"))
            RemCompDeferred(ent.Owner, ent.Comp);
    }

    private void OnBladeHarmfulAction(EntityUid uid, Component component, BeforeHarmfulActionEvent args)
    {
        if (args.Cancelled || args.Type == HarmfulActionType.Harm)
            return;

        args.Cancel();
    }

    private void OnBladeSlipAttempt(EntityUid uid, Component component, SlipAttemptEvent args)
    {
        args.NoSlip = true;
    }

    private void OnBeforeBladeStatusEffect(EntityUid uid, Component component, ref BeforeStatusEffectAddedEvent args)
    {
        if (args.Key is not ("KnockedDown" or "Stun"))
            return;

        args.Cancelled = true;
    }

    private void OnBeforeBladeStaminaDamage(EntityUid uid, Component component, ref BeforeStaminaDamageEvent args)
    {
        if (args.Value <= 0
            || args.Source == uid)
            return;

        args.Cancelled = true;
    }
}
