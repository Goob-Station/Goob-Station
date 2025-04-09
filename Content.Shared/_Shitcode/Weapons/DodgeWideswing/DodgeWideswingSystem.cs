// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Random;

namespace Content.Shared._Goobstation.Weapons.DodgeWideswing;

public sealed class DodgeWideswingSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DodgeWideswingComponent, BeforeDamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(EntityUid uid, DodgeWideswingComponent component, ref BeforeDamageChangedEvent args)
    {
        if (args.HeavyAttack && (!HasComp<KnockedDownComponent>(uid) || component.WhenKnockedDown) && _random.Prob(component.Chance))
        {
            _stamina.TakeStaminaDamage(uid, args.Damage.GetTotal().Float() * component.StaminaRatio, source: args.Origin, immediate: false);

            if (component.PopupId != null)
                _popup.PopupPredicted(Loc.GetString(component.PopupId, ("target", uid)), uid, args.Origin);

            args.Cancelled = true;
        }
    }
}