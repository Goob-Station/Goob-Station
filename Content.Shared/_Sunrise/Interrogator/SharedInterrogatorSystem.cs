using Content.Shared.Administration.Logs;
using Content.Shared.Body.Components;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.DragDrop;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;

namespace Content.Shared._Sunrise.Interrogator;

public abstract partial class SharedInterrogatorSystem: EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly SharedStandingStateSystem _standingStateSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedPointLightSystem _light = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InterrogatorComponent, CanDropTargetEvent>(OnInterrogatorCanDropOn);
    }

    protected void OnComponentInit(EntityUid uid, InterrogatorComponent interrogatorComponent, ComponentInit args)
    {
        interrogatorComponent.BodyContainer = _containerSystem.EnsureContainer<ContainerSlot>(uid, "body_container");
    }

    private void OnInterrogatorCanDropOn(EntityUid uid, InterrogatorComponent component, ref CanDropTargetEvent args)
    {
        if (args.Handled)
            return;

        args.CanDrop = HasComp<BodyComponent>(args.Dragged);
        args.Handled = true;
    }

    protected void UpdateAppearance(EntityUid uid, InterrogatorComponent? component = null, AppearanceComponent? appearance = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var interrogatorEnabled = HasComp<ActiveInterrogatorComponent>(uid);

        if (_light.TryGetLight(uid, out var light))
        {
            _light.SetEnabled(uid, interrogatorEnabled && component.BodyContainer.ContainedEntity != null, light);
        }

        if (!Resolve(uid, ref appearance))
            return;

        _appearanceSystem.SetData(uid, InterrogatorComponent.InterrogatorVisuals.ContainsEntity, component.BodyContainer.ContainedEntity == null, appearance);
        _appearanceSystem.SetData(uid, InterrogatorComponent.InterrogatorVisuals.IsOn, interrogatorEnabled, appearance);
    }

    public bool InsertBody(EntityUid uid, EntityUid target, InterrogatorComponent component)
    {
        if (component.BodyContainer.ContainedEntity != null)
            return false;

        if (!HasComp<MobStateComponent>(target))
            return false;

        var xform = Transform(target);
        _containerSystem.Insert((target, xform), component.BodyContainer);

        _standingStateSystem.Stand(target, force: true); // Force-stand the mob so that the cryo pod sprite overlays it fully

        UpdateAppearance(uid, component);
        return true;
    }

    private void TryEjectBody(EntityUid uid, EntityUid userId, InterrogatorComponent? component)
    {
        if (!Resolve(uid, ref component))
        {
            return;
        }

        var ejected = EjectBody(uid, component);
        if (ejected != null)
            _adminLogger.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(ejected.Value)} ejected from {ToPrettyString(uid)} by {ToPrettyString(userId)}");
    }

    protected virtual EntityUid? EjectBody(EntityUid uid, InterrogatorComponent? component)
    {
        if (!Resolve(uid, ref component))
            return null;

        if (component.BodyContainer.ContainedEntity is not {Valid: true} contained)
            return null;

        _containerSystem.Remove(contained, component.BodyContainer);
        // Insidecomponent is removed automatically in its EntGotRemovedFromContainerMessage listener
        // RemComp<Insidecomponent>(contained);

        // Restore the correct position of the patient. Checking the components manually feels hacky, but I did not find a better way for now.
        if (HasComp<KnockedDownComponent>(contained) || _mobStateSystem.IsIncapacitated(contained))
        {
            _standingStateSystem.Down(contained);
        }
        else
        {
            _standingStateSystem.Stand(contained);
        }

        UpdateAppearance(uid, component);
        return contained;
    }

    private void TryStartExtract(EntityUid uid, EntityUid userId, InterrogatorComponent? interrogatorComponent)
    {
        if (!Resolve(uid, ref interrogatorComponent))
        {
            return;
        }

        if (interrogatorComponent.BodyContainer.ContainedEntity == null)
            return;

        _adminLogger.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(uid)} start extract impants from {ToPrettyString(interrogatorComponent.BodyContainer.ContainedEntity)} by {ToPrettyString(userId)}");
        EnsureComp<ActiveInterrogatorComponent>(uid);
    }

    protected void AddAlternativeVerbs(EntityUid uid, InterrogatorComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        // Eject verb
        if (component.BodyContainer.ContainedEntity != null)
        {
            args.Verbs.Add(new AlternativeVerb
            {
                Text = Loc.GetString("interrogator-verb-noun-occupant"),
                Category = VerbCategory.Eject,
                Priority = 1,
                Act = () => TryEjectBody(uid, args.User, component)
            });
        }

        // Extract verb
        if (component.BodyContainer.ContainedEntity != null && !HasComp<ActiveInterrogatorComponent>(uid))
        {
            args.Verbs.Add(new AlternativeVerb
            {
                Text = Loc.GetString("interrogator-verb-start-extract"),
                Priority = 2,
                Act = () => TryStartExtract(uid, args.User, component)
            });
        }
    }

    [Serializable, NetSerializable]
    public sealed partial class InterrogatorDragFinished : SimpleDoAfterEvent
    {
    }
}
