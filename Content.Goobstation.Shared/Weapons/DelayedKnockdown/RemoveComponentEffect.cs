// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Weapons.DelayedKnockdown;

public sealed partial class RemoveComponentEffectSystem : EntityEffectSystem<MetaDataComponent, RemoveComponentEffect>// holy fuck holy fuck holy fuck oh my god kill this.
{
    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<RemoveComponentEffect> args)
    {
        if (string.IsNullOrEmpty(args.Effect.Component))
            return;

        var compType = EntityManager.ComponentFactory.GetRegistration(args.Effect.Component).Type;
        EntityManager.RemoveComponentDeferred(entity.Owner, compType);
    }
}

public sealed partial class RemoveComponentEffect : EntityEffectBase<RemoveComponentEffect>
{
    [DataField]
    public string? Locale;

    [DataField(required: true)]
    public string Component = string.Empty; // riders yelling at me

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Locale == null ? null : Loc.GetString(Locale);
    }
}
