// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.EntityEffects.Effects;

[DataDefinition]
public sealed partial class CreateEntityReactionEffect : EntityEffect
{
    /// <summary>
    ///     What entity to create.
    /// </summary>
    [DataField(required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string Entity = default!;

    /// <summary>
    ///     How many entities to create per unit reaction.
    /// </summary>
    [DataField]
    public uint Number = 1;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-create-entity-reaction-effect",
            ("chance", Probability),
            ("entname", IoCManager.Resolve<IPrototypeManager>().Index<EntityPrototype>(Entity).Name),
            ("amount", Number));

    public override void Effect(EntityEffectBaseArgs args)
    {
        var transform = args.EntityManager.GetComponent<TransformComponent>(args.TargetEntity);
        var transformSystem = args.EntityManager.System<SharedTransformSystem>();
        var quantity = (int)Number;
        if (args is EntityEffectReagentArgs reagentArgs)
            quantity *= reagentArgs.Quantity.Int();

        for (var i = 0; i < quantity; i++)
        {
            var uid = args.EntityManager.SpawnEntity(Entity, transformSystem.GetMapCoordinates(args.TargetEntity, xform: transform));
            transformSystem.AttachToGridOrMap(uid);

            // TODO figure out how to properly spawn inside of containers
            // e.g. cheese:
            // if the user is holding a bowl milk & enzyme, should drop to floor, not attached to the user.
            // if reaction happens in a backpack, should insert cheese into backpack.
            // --> if it doesn't fit, iterate through parent storage until it attaches to the grid (again, DON'T attach to players).
            // if the reaction happens INSIDE a stomach? the bloodstream? I have no idea how to handle that.
            // presumably having cheese materialize inside of your blood would have "disadvantages".
        }
    }
}