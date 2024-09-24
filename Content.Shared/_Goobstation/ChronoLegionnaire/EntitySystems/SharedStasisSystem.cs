using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Logs;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Database;
using Content.Shared.Emoting;
using Content.Shared._Goobstation.ChronoLegionnaire.Components;
using Content.Shared.Hands;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Movement.Events;
using Content.Shared.Pulling.Events;
using Content.Shared.Speech;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using Content.Shared.Storage.Components;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Shared._Goobstation.ChronoLegionnaire;

public abstract class SharedStasisSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<InsideStasisComponent, ComponentStartup>(OnStasisAdded);
        SubscribeLocalEvent<InsideStasisComponent, ComponentShutdown>(OnStasisRemoved);

        SubscribeLocalEvent<InsideStasisComponent, ChangeDirectionAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InsideStasisComponent, StandAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InsideStasisComponent, DownAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InsideStasisComponent, UseAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InsideStasisComponent, ThrowAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InsideStasisComponent, DropAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InsideStasisComponent, AttackAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InsideStasisComponent, PickupAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InsideStasisComponent, IsEquippingAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InsideStasisComponent, IsUnequippingAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InsideStasisComponent, StartPullAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InsideStasisComponent, SpeakAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<InsideStasisComponent, EmoteAttemptEvent>(OnAttempt);

        SubscribeLocalEvent<InsideStasisComponent, BeforeDamageChangedEvent>(OnDamage);
        SubscribeLocalEvent<InsideStasisComponent, UpdateCanMoveEvent>(OnMoveAttempt);
        SubscribeLocalEvent<InsideStasisComponent, InteractionAttemptEvent>(OnInteractionAttempt);
        SubscribeLocalEvent<InsideStasisComponent, GettingInteractedWithAttemptEvent>(OnInteractionWithAttempt);

        SubscribeLocalEvent<StasisProtectionComponent, GotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<StasisProtectionComponent, GotUnequippedEvent>(OnUnequip);

        SubscribeLocalEvent<StasisContainerComponent, StorageOpenAttemptEvent>(OnStorageOpenAttempt);
        SubscribeLocalEvent<StasisContainerComponent, ContainerIsInsertingAttemptEvent>(OnContained);
        SubscribeLocalEvent<StasisContainerComponent, ContainerIsRemovingAttemptEvent>(OnContainRemoved);
    }

    /// <summary>
    /// Disable the ability to move while adding an stasis effect
    /// </summary>
    protected void OnStasisAdded(Entity<InsideStasisComponent> stasised, ref ComponentStartup args)
    {
        var comp = stasised.Comp;

        _blocker.UpdateCanMove(stasised);
        _audio.PlayPredicted(comp.StasisSound, stasised, null);
        comp.Effect = Spawn(comp.EffectEntityProto, Transform(stasised.Owner).Coordinates);
        _transformSystem.SetParent(comp.Effect, stasised);
    }

    /// <summary>
    /// Enabling the ability to move while removing stasis effect
    /// </summary>
    protected void OnStasisRemoved(Entity<InsideStasisComponent> stasised, ref ComponentShutdown args)
    {
        var comp = stasised.Comp;

        _blocker.UpdateCanMove(stasised);
        _audio.PlayPredicted(comp.StasisEndSound, stasised, null);

        Del(comp.Effect);
    }

    /// <summary>
    /// Prevents entity in stasis get any damage (it's stuck in time, so no damage)
    /// </summary>
    public void OnDamage(Entity<InsideStasisComponent> stasised, ref BeforeDamageChangedEvent args)
    {
        args.Cancelled = true;
    }

    #region Cancelling interactions attemps

    /// <summary>
    /// Prevents any attempt by the player to do anything
    /// </summary>
    private void OnAttempt(EntityUid uid, InsideStasisComponent component, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }

    /// <summary>
    /// Prevents any player attempt to interact with world
    /// </summary>
    private void OnInteractionAttempt(Entity<InsideStasisComponent> stasised, ref InteractionAttemptEvent args)
    {
        args.Cancelled = true;
    }

    /// <summary>
    /// Prevents everyone except stasisImmune persons to interact with target in stasis (stripping/dragging/etc.)
    /// </summary>
    private void OnInteractionWithAttempt(Entity<InsideStasisComponent> stasised, ref GettingInteractedWithAttemptEvent args)
    {
        if (HasComp<StasisImmunityComponent>(args.Uid))
            return;

        args.Cancelled = true;
    }

    private void OnMoveAttempt(Entity<InsideStasisComponent> stasised, ref UpdateCanMoveEvent args)
    {
        // Check to return the player's ability to move as soon as possible
        if (stasised.Comp.LifeStage > ComponentLifeStage.Running)
            return;

        args.Cancel();
    }

    #endregion

    public bool TryStasis(Entity<StatusEffectsComponent?> target, bool refresh, TimeSpan? time = null)
    {
        var statusTime = time;
        var comp = target.Comp;

        // One day timespan required for stasis container
        if (statusTime == null)
            statusTime = new TimeSpan(1, 0, 0, 0, 0);

        if (!Resolve(target, ref comp))
            return false;

        if (HasComp<StasisImmunityComponent>(target))
            return false;

        if (!_statusEffects.TryAddStatusEffect<InsideStasisComponent>(target, "Stasis", statusTime.Value, refresh))
            return false;

        var ev = new StasisEvent();
        RaiseLocalEvent(target, ref ev);

        _adminLogger.Add(LogType.Stamina, LogImpact.Medium, $"{ToPrettyString(target):entity} was send into stasis");

        return true;
    }

    #region Stasis protection
    public void OnEquip(Entity<StasisProtectionComponent> protection, ref GotEquippedEvent args)
    {
        // Making x10 staminaDamage to make sure no one stunbaton them (until stun resist will be added)
        if (TryComp<StaminaComponent>(args.Equipee, out var staminaComp))
        {
            staminaComp.CritThreshold *= protection.Comp.StaminaModifier;
            staminaComp.Decay *= protection.Comp.StaminaModifier;
        }

        // Applying stasis immune
        EnsureComp<StasisImmunityComponent>(args.Equipee);
    }

    public void OnUnequip(Entity<StasisProtectionComponent> protection, ref GotUnequippedEvent args)
    {
        if (TryComp<StaminaComponent>(args.Equipee, out var staminaComp))
        {
            staminaComp.CritThreshold /= protection.Comp.StaminaModifier;
            staminaComp.Decay /= protection.Comp.StaminaModifier;
        }

        if (HasComp<StasisImmunityComponent>(args.Equipee) )
            EntityManager.RemoveComponent<StasisImmunityComponent>(args.Equipee);
    }

    #endregion

    /// <summary>
    /// Preventing open container from inside
    /// </summary>
    private void OnStorageOpenAttempt(Entity<StasisContainerComponent> stasised, ref StorageOpenAttemptEvent args)
    {
        if (HasComp<InsideStasisComponent>(args.User))
            args.Cancelled = true;
    }

    /// <summary>
    /// Try to apply stasis on the target when it in the stasis container
    /// </summary>
    public void OnContained(Entity<StasisContainerComponent> container, ref ContainerIsInsertingAttemptEvent args)
    {
        if (HasComp<StasisImmunityComponent>(args.EntityUid))
            return;

        TryStasis((args.EntityUid, null), true, null);
    }

    /// <summary>
    /// Removing stasis effect if target left container
    /// </summary>
    public void OnContainRemoved(Entity<StasisContainerComponent> container, ref ContainerIsRemovingAttemptEvent args)
    {
        _statusEffects.TryRemoveStatusEffect(args.EntityUid, "Stasis");
    }
}
