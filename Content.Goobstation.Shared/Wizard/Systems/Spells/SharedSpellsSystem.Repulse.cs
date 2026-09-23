using System.Numerics;
using Content.Goobstation.Shared.Wizard.Events;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private void OnRepulse(RepulseEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        var mapPos = _xform.GetMapCoordinates(ev.Performer);

        if (mapPos == MapCoordinates.Nullspace)
            return;

        var baseMatrixDeltaV = new Matrix3x2(-ev.Force, 0f, 0f, -ev.Force, 0f, 0f);
        var epicenter = mapPos.Position;
        var minRange2 = ev.MinRange * ev.MinRange;

        foreach (Entity<PhysicsComponent> ent in _lookup.GetEntitiesInRange<PhysicsComponent>(mapPos,
                     ev.MaxRange,
                     flags: LookupFlags.Dynamic | LookupFlags.Sundries))
        {
            if (ent.Comp.BodyType == BodyType.Static)
                continue;

            if (ent.Owner == ev.Performer)
                continue;

            if (_divineIntervention.TouchSpellDenied(ent))
                continue;

            // TODO: stupid thing is Server, only thing preventing this from being predicted
            // therefore,  i dont Care. its a wiz spell. but if this becomes predicted, maybe uncomment this
            //if (!_gravityWell.CanGravPulseAffect(ent))
            //    continue;

            var xform = _xformQuery.Comp(ent);

            var displacement = epicenter - _xform.GetWorldPosition(xform, _xformQuery);
            var distance2 = displacement.LengthSquared();
            if (distance2 < minRange2)
                continue;

            _stun.TryUpdateParalyzeDuration(ent, ev.StunTime);

            PredictedSpawnAtPosition(ev.EffectProto, xform.Coordinates);

            var scaling = 1f / distance2 * ent.Comp.Mass;
            _physics.ApplyLinearImpulse(ent,
                Vector2.TransformNormal(displacement, baseMatrixDeltaV) * scaling,
                body: ent);
        }

        ev.Handled = true;
    }
}