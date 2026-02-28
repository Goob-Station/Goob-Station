// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics;
using System.Linq;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Goobstation.Shared.EntityEffects;

/// <summary>
///     Saturates the lungs of nearby respirators.
/// </summary>
public sealed partial class OxygenateNearby : EventEntityEffect<OxygenateNearby>
{

    [DataField]
    public float Range = 7;

    [DataField]
    public float Factor = 10f;

    public OxygenateNearby(float range, float factor)
    {
        Range = range;
        Factor = factor;
    }

    public override bool ShouldLog => true;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-ignite", ("chance", Probability)); //In due time...

    public override LogImpact LogImpact => LogImpact.Medium;

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not EntityEffectReagentArgs reagentArgs)
            return;

        var ev = new OxygenateNearby(Range, Factor);
        args.EntityManager.EventBus.RaiseLocalEvent(args.TargetEntity, ev);


    }
}
