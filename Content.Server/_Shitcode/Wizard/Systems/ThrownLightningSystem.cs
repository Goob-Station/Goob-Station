using Content.Server._Goobstation.Wizard.Components;
using Content.Server.Electrocution;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared._Goobstation.Wizard.Traps;
using Content.Shared.Damage.Systems;
using Content.Shared.Magic.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Throwing;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class ThrownLightningSystem : EntitySystem
{
    [Dependency] private readonly ElectrocutionSystem _electrocution = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SpellsSystem _spells = default!;
    [Dependency] private readonly SparksSystem _sparks = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrownLightningComponent, ThrowDoHitEvent>(OnHit);
        SubscribeLocalEvent<ThrownLightningComponent, ThrownEvent>(OnThrown);
        SubscribeLocalEvent<ThrownLightningComponent, StopThrowEvent>(OnStopThrow);
    }

    private void OnStopThrow(Entity<ThrownLightningComponent> ent, ref StopThrowEvent args)
    {
        if (Deleting(ent))
            return;

        if (!TryComp(ent, out TrailComponent? trail))
            return;

        trail.ParticleAmount = 0;
        Dirty(ent.Owner, trail);
    }

    private void OnThrown(Entity<ThrownLightningComponent> ent, ref ThrownEvent args)
    {
        if (TryComp(ent, out TrailComponent? trail))
        {
            trail.ParticleAmount = 1;
            Dirty(ent.Owner, trail);
        }

        if (args.User == null)
            return;

        var speech = ent.Comp.Speech == null ? string.Empty : Loc.GetString(ent.Comp.Speech);
        _spells.SpeakSpell(args.User.Value, args.User.Value, speech, MagicSchool.Conjuration);
    }

    private void OnHit(Entity<ThrownLightningComponent> ent, ref ThrowDoHitEvent args)
    {
        if (Deleting(ent))
            return;

        if (args.Handled)
            return;

        args.Handled = true;

        if (!TryComp(args.Target, out StatusEffectsComponent? status))
            return;

        _electrocution.TryDoElectrocution(args.Target, ent, 2, TimeSpan.Zero, true, 0.5f, status, true);
        _stamina.TakeStaminaDamage(args.Target, ent.Comp.StaminaDamage);
        _sparks.DoSparks(Transform(ent).Coordinates);
    }

    private bool Deleting(EntityUid ent)
    {
        return EntityManager.IsQueuedForDeletion(ent) || TerminatingOrDeleted(ent);
    }
}
