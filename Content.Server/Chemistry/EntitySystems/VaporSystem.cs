// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 TekuNut <13456422+TekuNut@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Chemistry.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.Physics;
using Content.Shared.Throwing;
using Content.Shared.Chemistry.EntitySystems;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;
using System.Numerics;

namespace Content.Server.Chemistry.EntitySystems
{
    [UsedImplicitly]
    internal sealed class VaporSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _protoManager = default!;
        [Dependency] private readonly SharedMapSystem _map = default!;
        [Dependency] private readonly SharedPhysicsSystem _physics = default!;
        [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
        [Dependency] private readonly ThrowingSystem _throwing = default!;
        [Dependency] private readonly ReactiveSystem _reactive = default!;
        [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

        private const float ReactTime = 0.125f;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<VaporComponent, StartCollideEvent>(HandleCollide);
        }

        private void HandleCollide(Entity<VaporComponent> entity, ref StartCollideEvent args)
        {
            if (!EntityManager.TryGetComponent(entity.Owner, out SolutionContainerManagerComponent? contents)) return;

            foreach (var (_, soln) in _solutionContainerSystem.EnumerateSolutions((entity.Owner, contents)))
            {
                var solution = soln.Comp.Solution;
                _reactive.DoEntityReaction(args.OtherEntity, solution, ReactionMethod.Touch);
            }

            // Check for collision with a impassable object (e.g. wall) and stop
            if ((args.OtherFixture.CollisionLayer & (int) CollisionGroup.Impassable) != 0 && args.OtherFixture.Hard)
            {
                EntityManager.QueueDeleteEntity(entity);
            }
        }

        public void Start(Entity<VaporComponent> vapor, TransformComponent vaporXform, Vector2 dir, float speed, MapCoordinates target, float aliveTime, EntityUid? user = null)
        {
            vapor.Comp.Active = true;
            var despawn = EnsureComp<TimedDespawnComponent>(vapor);
            despawn.Lifetime = aliveTime;

            // Set Move
            if (EntityManager.TryGetComponent(vapor, out PhysicsComponent? physics))
            {
                _physics.SetLinearDamping(vapor, physics, 0f);
                _physics.SetAngularDamping(vapor, physics, 0f);

                _throwing.TryThrow(vapor, dir, speed, user: user);

                var distance = (target.Position - _transformSystem.GetWorldPosition(vaporXform)).Length();
                var time = (distance / physics.LinearVelocity.Length());
                despawn.Lifetime = MathF.Min(aliveTime, time);
            }
        }

        internal bool TryAddSolution(Entity<VaporComponent> vapor, Solution solution)
        {
            if (solution.Volume == 0)
            {
                return false;
            }

            if (!_solutionContainerSystem.TryGetSolution(vapor.Owner, VaporComponent.SolutionName, out var vaporSolution))
            {
                return false;
            }

            return _solutionContainerSystem.TryAddSolution(vaporSolution.Value, solution);
        }

        public override void Update(float frameTime)
        {
            var query = EntityQueryEnumerator<VaporComponent, SolutionContainerManagerComponent, TransformComponent>();
            while (query.MoveNext(out var uid, out var vaporComp, out var container, out var xform))
            {
                foreach (var (_, soln) in _solutionContainerSystem.EnumerateSolutions((uid, container)))
                {
                    Update(frameTime, (uid, vaporComp), soln, xform);
                }
            }
        }

        private void Update(float frameTime, Entity<VaporComponent> ent, Entity<SolutionComponent> soln, TransformComponent xform)
        {
            var (entity, vapor) = ent;
            if (!vapor.Active)
                return;

            vapor.ReactTimer += frameTime;

            var contents = soln.Comp.Solution;
            if (vapor.ReactTimer >= ReactTime && TryComp(xform.GridUid, out MapGridComponent? gridComp))
            {
                vapor.ReactTimer = 0;

                var tile = _map.GetTileRef(xform.GridUid.Value, gridComp, xform.Coordinates);
                foreach (var reagentQuantity in contents.Contents.ToArray())
                {
                    if (reagentQuantity.Quantity == FixedPoint2.Zero) continue;
                    var reagent = _protoManager.Index<ReagentPrototype>(reagentQuantity.Reagent.Prototype);

                    var reaction =
                        reagent.ReactionTile(tile, (reagentQuantity.Quantity / vapor.TransferAmount) * 0.25f, EntityManager, reagentQuantity.Reagent.Data);

                    if (reaction > reagentQuantity.Quantity)
                    {
                        Log.Error($"Tried to tile react more than we have for reagent {reagentQuantity}. Found {reaction} and we only have {reagentQuantity.Quantity}");
                        reaction = reagentQuantity.Quantity;
                    }

                    _solutionContainerSystem.RemoveReagent(soln, reagentQuantity.Reagent, reaction);
                }
            }

            if (contents.Volume == 0)
            {
                // Delete this
                EntityManager.QueueDeleteEntity(entity);
            }
        }
    }
}