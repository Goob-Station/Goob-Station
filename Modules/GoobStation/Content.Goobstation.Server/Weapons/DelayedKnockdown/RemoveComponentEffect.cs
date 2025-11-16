// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Weapons.DelayedKnockdown;

public sealed partial class RemoveComponentEffect : EntityEffect
{
    [DataField]
    public string? Locale;

    [DataField(required: true)]
    public string Component = string.Empty; // riders yelling at me

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Locale == null ? null : Loc.GetString(Locale);
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        args.EntityManager.RemoveComponentDeferred(args.TargetEntity,
            args.EntityManager.ComponentFactory.GetRegistration(Component).Type);
    }
}