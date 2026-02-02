using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Goobstation.Server.EntityEffects;
public sealed partial class ApplyHandcuffsEntityEffect : EntityEffect
{
    public override void Effect(EntityEffectBaseArgs args)
    {
        // TODO
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
