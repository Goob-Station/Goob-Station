// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Religion;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;

namespace Content.Goobstation.Shared.Religion;

public sealed class HolyResistanceSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HolyResistanceComponent, DamageModifyEvent>(OnDamageModify); // goob edit - why hasn't anyone done this yet?>
    }

    private static Dictionary<string, float> _resistanceCoefficientDict = new()
    {
        { "Holy", 0 },
    };

    // Create a modifier set
    DamageModifierSetPrototype holymodifierSet = new()
    {
        Coefficients = _resistanceCoefficientDict
    };

    private void OnDamageModify(EntityUid uid, HolyResistanceComponent component, DamageModifyEvent args)
    {
        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, holymodifierSet);
    }

}
