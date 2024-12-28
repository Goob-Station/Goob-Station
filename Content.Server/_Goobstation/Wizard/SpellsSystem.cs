using System.Numerics;
using Content.Server.Abilities.Mime;
using Content.Server.Administration.Commands;
using Content.Server.Emp;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Singularity.EntitySystems;
using Content.Server.Spreader;
using Content.Shared._Goobstation.Wizard;
using Content.Shared.Chemistry.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Maps;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffect;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;

namespace Content.Server._Goobstation.Wizard;

public sealed class SpellsSystem : SharedSpellsSystem
{
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly SpreaderSystem _spreader = default!;
    [Dependency] private readonly GravityWellSystem _gravityWell = default!;

    protected override void SetGear(EntityUid uid, string gear, SlotFlags unremoveableClothingFlags = SlotFlags.NONE)
    {
        base.SetGear(uid, gear, unremoveableClothingFlags);

        SetOutfitCommand.SetOutfit(uid, gear, EntityManager);

        if (unremoveableClothingFlags == SlotFlags.NONE)
            return;

        var enumerator = Inventory.GetSlotEnumerator(uid, unremoveableClothingFlags);
        while (enumerator.MoveNext(out var container))
        {
            if (HasComp<ClothingComponent>(container.ContainedEntity))
                EnsureComp<UnremoveableComponent>(container.ContainedEntity.Value);
        }
    }

    protected override void MakeMime(MimeMalaiseEvent ev, StatusEffectsComponent? status = null)
    {
        base.MakeMime(ev, status);

        var targetWizard = HasComp<WizardComponent>(ev.Target);

        SetGear(ev.Target,
            ev.Gear,
            targetWizard ? SlotFlags.NONE : SlotFlags.MASK | SlotFlags.INNERCLOTHING | SlotFlags.BELT);

        if (!targetWizard)
            EnsureComp<MimePowersComponent>(ev.Target).CanBreakVow = false;
        else
            StatusEffects.TryAddStatusEffect<MutedComponent>(ev.Target, "Muted", ev.WizardMuteDuration, true, status);
    }

    protected override void Emp(DisableTechEvent ev)
    {
        base.Emp(ev);

        // This doesn't invoke EmpPulse() because I don't want it to spawn emp effect and play pulse sound
        var coords = TransformSystem.GetMapCoordinates(ev.Performer);
        foreach (var uid in Lookup.GetEntitiesInRange(coords, ev.Range))
        {
            _emp.TryEmpEffects(uid, ev.EnergyConsumption, ev.DisableDuration);
        }
    }

    protected override void SpawnSmoke(SmokeSpellEvent ev)
    {
        base.SpawnSmoke(ev);

        var xform = Transform(ev.Performer);
        var mapCoords = TransformSystem.GetMapCoordinates(ev.Performer, xform);

        if (!MapManager.TryFindGridAt(mapCoords, out var gridUid, out var grid) ||
            !Map.TryGetTileRef(gridUid, grid, xform.Coordinates, out var tileRef) ||
            tileRef.Tile.IsEmpty)
            return;

        if (_spreader.RequiresFloorToSpread(ev.Proto.ToString()) && tileRef.Tile.IsSpace())
            return;

        var coords = Map.MapToGrid(gridUid, mapCoords);
        var ent = Spawn(ev.Proto, coords.SnapToGrid());
        if (!TryComp<SmokeComponent>(ent, out var smoke))
        {
            Log.Error($"Smoke prototype {ev.Proto} was missing SmokeComponent");
            Del(ent);
            return;
        }

        _smoke.StartSmoke(ent, new Solution(ev.Solution), ev.Duration, ev.SpreadAmount, smoke);
    }

    protected override void Repulse(RepulseEvent ev)
    {
        var mapPos = TransformSystem.GetMapCoordinates(ev.Performer);

        if (mapPos == MapCoordinates.Nullspace)
            return;

        var baseMatrixDeltaV = new Matrix3x2(-ev.Force, 0f, 0f, -ev.Force, 0f, 0f);
        var epicenter = mapPos.Position;
        var minRange2 = ev.MinRange * ev.MinRange;
        var xformQuery = GetEntityQuery<TransformComponent>();

        foreach (var (entity, physics) in Lookup.GetEntitiesInRange<PhysicsComponent>(mapPos,
                     ev.MaxRange,
                     flags: LookupFlags.Dynamic | LookupFlags.Sundries))
        {
            if (physics.BodyType == BodyType.Static)
                continue;

            if (entity == ev.Performer)
                continue;

            if (!_gravityWell.CanGravPulseAffect(entity))
                continue;

            var xform = xformQuery.Comp(entity);

            var displacement = epicenter - TransformSystem.GetWorldPosition(xform, xformQuery);
            var distance2 = displacement.LengthSquared();
            if (distance2 < minRange2)
                continue;

            Stun.TryParalyze(entity, ev.StunTime, true);

            if (ev.EffectProto != null)
            {
                var effect = SpawnAttachedTo(ev.EffectProto.Value, xform.Coordinates);
                TransformSystem.SetParent(effect, xformQuery.Comp(effect), entity, xform);
            }

            var scaling = (1f / distance2) * physics.Mass;
            Physics.ApplyLinearImpulse(entity,
                Vector2.TransformNormal(displacement, baseMatrixDeltaV) * scaling,
                body: physics);
        }
    }
}
