// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

/// <summary>
///     Creates smoke similar to SmokeOnTrigger
/// </summary>

// i dont even know if this works. if you're reading this, it likely doesn't. Change the Comp.
public sealed partial class DoSmokeEntityEffectSystem : EntityEffectSystem<ReactiveComponent, DoSmokeEntityEffect>
{
    protected override void Effect(Entity<ReactiveComponent> entity, ref EntityEffectEvent<DoSmokeEntityEffect> args)
    {
        var ev = new DoSmokeEntityEffect(
            args.Effect.Duration,
            args.Effect.SpreadAmount,
            args.Effect.SmokePrototype,
            args.Effect.Solution);

        EntityManager.EventBus.RaiseLocalEvent(entity.Owner, ev);
    }
}

public sealed partial class DoSmokeEntityEffect : EntityEffectBase<DoSmokeEntityEffect>
{
    /// <summary>
    /// How long the smoke stays for, after it has spread.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Duration = 10;

    /// <summary>
    /// How much the smoke will spread.
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public int SpreadAmount;

    /// <summary>
    /// Smoke entity to spawn.
    /// Defaults to smoke but you can use foam if you want.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId SmokePrototype = "Smoke";

    /// <summary>
    /// Solution to add to each smoke cloud.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Solution Solution = new();

    public DoSmokeEntityEffect(float duration, int spreadAmount, EntProtoId smokePrototype, Solution solution)
    {
        Duration = duration;
        SpreadAmount = spreadAmount;
        SmokePrototype = smokePrototype;
        Solution = solution;
    }

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;

    public override LogImpact LogImpact => LogImpact.Medium;
}
