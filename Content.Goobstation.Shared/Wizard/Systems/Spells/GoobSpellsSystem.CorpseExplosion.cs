using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Gibbing.Events;

namespace Content.Goobstation.Shared.Wizard.Systems;

public sealed partial class SharedGoobSpellsSystem
{

    private void OnCorpseExplosion(CorpseExplosionEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (HasComp<SiliconComponent>(ev.Target))
        {
            _popup.PopupClient(Loc.GetString(_locFailSilicon), ev.Performer);
            return;
        }

        if (!_mobState.IsDead(ev.Target))
        {
            _popup.PopupClient(Loc.GetString(_locFailNotDead), ev.Performer);
            return;
        }

        // Only visual
        _explosion.QueueExplosion(ev.Target,
            ev.ExplosionId,
            ev.TotalIntensity,
            ev.Slope,
            ev.MaxIntenity,
            0f,
            0,
            false,
            ev.Performer);

        if (_timing.IsFirstTimePredicted)
            _body.GibBody(ev.Target, contents: GibContentsOption.Gib);

        var coords = _xform.GetMapCoordinates(ev.Target);
        var targets = _lookup.GetEntitiesInRange<DamageableComponent>(coords, ev.KnockdownRange);

        foreach (var (target, damageable) in targets)
        {
            if (target == ev.Performer || target == ev.Target)
                continue;

            if (_spectralQuery.HasComp(target))
                continue;

            var range = (_xform.GetMapCoordinates(target).Position - coords.Position).Length();

            range = MathF.Max(1f, range);

            _damageable.TryChangeDamage(target,
                ev.Damage / range,
                damageable: damageable,
                origin: ev.Performer,
                targetPart: TargetBodyPart.All);

            if (HasComp<SiliconComponent>(target))
                _stun.TryUpdateParalyzeDuration(target, ev.SiliconStunTime / range);
            else
                _stun.KnockdownOrStun(target, ev.KnockdownTime / range, true);
        }

        ev.Handled = true;
    }
}
