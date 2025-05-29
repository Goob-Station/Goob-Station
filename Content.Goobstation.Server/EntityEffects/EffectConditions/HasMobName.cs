// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 ImWeax <59857479+ImWeax@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.IdentityManagement;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects.EffectConditions;

public sealed partial class HasMobName : EntityEffectCondition
{
    [DataField]
    public string? Name = null;

    public override bool Condition(EntityEffectBaseArgs args)
    {
            return Name!= null && Identity.Name(args.TargetEntity, args.EntityManager).ToLower().Contains(Name.ToLower());
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        Name ??= "";
        return Loc.GetString("reagent-effect-condition-guidebook-has-mob-name",("name",Name));
    }
}
