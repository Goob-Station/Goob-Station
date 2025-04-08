// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 areit <77842470+areitpog@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Stunnable;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

public sealed partial class Paralyze : EntityEffect
{
    [DataField] public double ParalyzeTime = 2;

    /// <remarks>
    ///     true - refresh paralyze time,  false - accumulate paralyze time
    /// </remarks>
    [DataField] public bool Refresh = true;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-paralyze",
            ("chance", Probability),
            ("time", ParalyzeTime));

    public override void Effect(EntityEffectBaseArgs args)
    {
        var paralyzeTime = ParalyzeTime;

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            paralyzeTime *= (double)reagentArgs.Scale;
        }

        args.EntityManager.System<StunSystem>().TryParalyze(args.TargetEntity, TimeSpan.FromSeconds(paralyzeTime), Refresh);
    }
}
