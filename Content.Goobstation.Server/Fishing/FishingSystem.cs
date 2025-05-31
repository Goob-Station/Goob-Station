// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.Fishing.Components;
using Content.Goobstation.Shared.Fishing.Systems;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Physics;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Fishing;

public sealed class FishingSystem : SharedFishingSystem
{
    // Here we calculate the start of fishing, because apparently StartCollideEvent
    // works janky on clientside so we can't predict when fishing starts.
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FishingLureComponent, StartCollideEvent>(OnFloatCollide);
        SubscribeLocalEvent<FishingRodComponent, UseInHandEvent>(OnFishingInteract);
    }

    #region Event handling

    private void OnFloatCollide(Entity<FishingLureComponent> ent, ref StartCollideEvent args)
    {
        // TODO:  make it so this can collide with any unacnchored objects (items, mobs, etc) but not the player casting it (get parent of rod?)
        // Fishing spot logic
        var attachedEnt = args.OtherEntity;

        if (HasComp<ActiveFishingSpotComponent>(attachedEnt))
            return;

        if (!FishSpotQuery.TryComp(attachedEnt, out var spotComp))
        {
            if (args.OtherBody.BodyType == BodyType.Static)
                return;

            Anchor(ent, attachedEnt);
            return;
        }

        // Anchor fishing float on an entity
        Anchor(ent, attachedEnt);

        // Currently we don't support multiple loots from this
        var fish = spotComp.FishList.GetSpawns(_random.GetRandom(), EntityManager, _proto).First();

        // Get fish difficulty
        _proto.Index(fish).TryGetComponent(out FishComponent? fishComp, _compFactory);

        // Assign things that depend on the fish
        var activeFishSpot = EnsureComp<ActiveFishingSpotComponent>(attachedEnt);
        activeFishSpot.Fish = fish;
        activeFishSpot.FishDifficulty = fishComp?.FishDifficulty ?? FishComponent.DefaultDifficulty;

        // Assign things that depend on the spot
        var time = spotComp.FishDefaultTimer + _random.NextFloat(-spotComp.FishTimerVariety, spotComp.FishTimerVariety);
        activeFishSpot.FishingStartTime = Timing.CurTime + TimeSpan.FromSeconds(time);
        activeFishSpot.AttachedFishingLure = ent;

        // Declares war on prediction
        Dirty(attachedEnt, activeFishSpot);
        Dirty(ent);
    }

    private void OnFishingInteract(EntityUid uid, FishingRodComponent component, UseInHandEvent args)
    {
        if (!FisherQuery.TryComp(args.User, out var fisherComp) || fisherComp.TotalProgress == null || args.Handled || !Timing.IsFirstTimePredicted)
            return;

        fisherComp.TotalProgress += fisherComp.ProgressPerUse * component.Efficiency;
        Dirty(args.User, fisherComp); // That's a bit evil, but we want to keep numbers real.

        args.Handled = true;
    }

    private void Anchor(Entity<FishingLureComponent> ent, EntityUid attachedEnt)
    {
        var spotPosition = Xform.GetWorldPosition(attachedEnt);
        Xform.SetWorldPosition(ent, spotPosition);
        Xform.SetParent(ent, attachedEnt);
        _physics.SetLinearVelocity(ent, Vector2.Zero);
        _physics.SetAngularVelocity(ent, 0f);
        ent.Comp.AttachedEntity = attachedEnt;
        RemComp<ItemComponent>(ent);
        RemComp<PullableComponent>(ent);
    }

    #endregion

    protected override void SetupFishingFloat(Entity<FishingRodComponent> fishingRod, EntityUid player, EntityCoordinates target)
    {
        var (uid, component) = fishingRod;
        var targetCoords = Xform.ToMapCoordinates(target);
        var playerCoords = Xform.GetMapCoordinates(Transform(player));

        var fishFloat = Spawn(component.FloatPrototype, playerCoords);
        component.FishingLure = fishFloat;
        Dirty(uid, component);

        // Calculate throw direction
        var direction = targetCoords.Position - playerCoords.Position;
        if (direction == Vector2.Zero)
            direction = Vector2.UnitX; // If the user somehow manages to click directly in the center of themself, just toss it to the right i guess.

        // Yeet
        Throwing.TryThrow(fishFloat, direction, 15f, player, 2f, null, true);

        // Set up lure component
        var fishLureComp = EnsureComp<FishingLureComponent>(fishFloat);
        fishLureComp.FishingRod = uid;
        Dirty(fishFloat, fishLureComp);

        // Rope visuals
        var visuals = EnsureComp<JointVisualsComponent>(fishFloat);
        visuals.Sprite = component.RopeSprite;
        visuals.OffsetA = component.RopeLureOffset;
        visuals.OffsetB = component.RopeUserOffset;
        visuals.Target = GetNetEntity(uid);
    }

    protected override void ThrowFishReward(EntProtoId fishId, EntityUid fishSpot, EntityUid target)
    {
        var position = Transform(fishSpot).Coordinates;
        var fish = Spawn(fishId, position);
        // Throw da fish back to the player because it looks funny
        var direction = Xform.GetWorldPosition(target) - Xform.GetWorldPosition(fish);
        var length = direction.Length();
        var distance = Math.Clamp(length, 0.5f, 15f);
        direction *= distance / length;

        Throwing.TryThrow(fish, direction, 7f);
    }

    protected override void CalculateFightingTimings(Entity<ActiveFisherComponent> fisher, ActiveFishingSpotComponent activeSpotComp)
    {
        if (Timing.CurTime < fisher.Comp.NextStruggle)
            return;

        fisher.Comp.NextStruggle = Timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(0.06f, 0.18f));
        fisher.Comp.TotalProgress -= activeSpotComp.FishDifficulty;
        Dirty(fisher);
    }
}
