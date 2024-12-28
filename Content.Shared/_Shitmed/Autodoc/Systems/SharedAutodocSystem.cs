using Content.Shared._Shitmed.Autodoc.Components;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared._Shitmed.Medical.Surgery.Steps;
using Content.Shared.Administration.Logs;
using Content.Shared.Bed.Sleep;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Buckle.Components;
using Content.Shared.Database;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Labels.EntitySystems;
using Content.Shared.Mobs.Systems;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Shared._Shitmed.Autodoc.Systems;

public abstract class SharedAutodocSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedLabelSystem _label = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly SharedSurgerySystem _surgery = default!;
    [Dependency] private readonly SleepingSystem _sleeping = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AutodocComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<AutodocComponent, PortDisconnectedEvent>(OnPortDisconnected);
        Subs.BuiEvents<AutodocComponent>(AutodocUiKey.Key, s =>
        {
            s.Event<AutodocCreateProgramMessage>(OnCreateProgram);
            s.Event<AutodocToggleProgramSafetyMessage>(OnToggleProgramSafety);
            s.Event<AutodocRemoveProgramMessage>(OnRemoveProgram);
            s.Event<AutodocAddStepMessage>(OnAddStep);
            s.Event<AutodocRemoveStepMessage>(OnRemoveStep);
            s.Event<AutodocStartMessage>(OnStart);
            s.Event<AutodocStopMessage>(OnStop);
        });

        SubscribeLocalEvent<ActiveAutodocComponent, SurgeryStepEvent>(OnSurgeryStep);
        SubscribeLocalEvent<ActiveAutodocComponent, SurgeryStepFailedEvent>(OnSurgeryStepFailed);
        SubscribeLocalEvent<ActiveAutodocComponent, ComponentShutdown>(OnActiveShutdown);
    }

    private void OnNewLink(Entity<AutodocComponent> ent, ref NewLinkEvent args)
    {
        if (args.SinkPort == ent.Comp.OperatingTablePort &&
            HasComp<OperatingTableComponent>(args.Source))
        {
            ent.Comp.OperatingTable = args.Source;
            Dirty(ent);
        }
    }

    private void OnPortDisconnected(Entity<AutodocComponent> ent, ref PortDisconnectedEvent args)
    {
        if (args.Port != ent.Comp.OperatingTablePort)
            return;

        ent.Comp.OperatingTable = null;
        Dirty(ent);
    }

    #region UI Handling

    private void OnCreateProgram(Entity<AutodocComponent> ent, ref AutodocCreateProgramMessage args)
    {
        CreateProgram(ent, args.Title);
    }

    private void OnToggleProgramSafety(Entity<AutodocComponent> ent, ref AutodocToggleProgramSafetyMessage args)
    {
        if (IsActive(ent))
            return;

        if (args.Program >= ent.Comp.Programs.Count)
            return;

        var program = ent.Comp.Programs[args.Program];
        program.SkipFailed ^= true;
        Dirty(ent);

        _adminLogger.Add(LogType.InteractActivate, LogImpact.Low, $"{ToPrettyString(args.Actor):user} toggled safety of autodoc program {program.Title}");
    }

    private void OnRemoveProgram(Entity<AutodocComponent> ent, ref AutodocRemoveProgramMessage args)
    {
        RemoveProgram(ent, args.Program);
    }

    private void OnAddStep(Entity<AutodocComponent> ent, ref AutodocAddStepMessage args)
    {
        if (!args.Step.Validate(ent, this))
        {
            Log.Warning($"User {ToPrettyString(args.Actor)} tried to add an invalid autodoc step!");
            return;
        }

        AddStep(ent, args.Program, args.Step, args.Index, args.Actor);
    }

    private void OnRemoveStep(Entity<AutodocComponent> ent, ref AutodocRemoveStepMessage args)
    {
        RemoveStep(ent, args.Program, args.Step);
    }

    private void OnStart(Entity<AutodocComponent> ent, ref AutodocStartMessage args)
    {
        StartProgram(ent, args.Program, args.Actor);
    }

    private void OnStop(Entity<AutodocComponent> ent, ref AutodocStopMessage args)
    {
        RemComp<ActiveAutodocComponent>(ent);
    }

    #endregion

    private void OnSurgeryStep(Entity<ActiveAutodocComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp<AutodocComponent>(ent, out var comp))
            return;

        var repeatable = HasComp<SurgeryRepeatableStepComponent>(args.Step);
        if (args.Complete || !repeatable)
        {
            ent.Comp.Waiting = false; // try the next autodoc or surgery step
            return;
        }

        // for tend wounds dont abort, more wounds need tending
        if (HasComp<SurgeryRepeatableStepComponent>(args.Step))
            return;

        ent.Comp.Waiting = repeatable;
    }

    private void OnSurgeryStepFailed(Entity<ActiveAutodocComponent> ent, ref SurgeryStepFailedEvent args)
    {
        if (!TryComp<AutodocComponent>(ent, out var comp))
            return;

        var program = comp.Programs[ent.Comp.CurrentProgram];
        var error = Loc.GetString("autodoc-error-surgery-failed");
        if (program.SkipFailed)
        {
            Say(ent, Loc.GetString("autodoc-error", ("error", error)));
            ent.Comp.ProgramStep++;
        }
        else
        {
            Say(ent, Loc.GetString("autodoc-fatal-error", ("error", error)));
            RemCompDeferred<ActiveAutodocComponent>(ent);
        }
    }

    private void OnActiveShutdown(Entity<ActiveAutodocComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<AutodocComponent>(ent, out var comp))
            return;

        // wake the patient when program completes or errors out
        if (GetPatient((ent.Owner, comp)) is {} patient)
            WakePatient(patient);
    }

    protected virtual void WakePatient(EntityUid patient)
    {
        _sleeping.TryWaking(patient);
    }

    #region Step API

    public bool IsSurgery(EntProtoId id)
    {
        // this is O(n) so with a fuck ton of surgeries it could slow down the server
        return _surgery.AllSurgeries.Contains(id);
    }

    public EntityUid? FindItem(EntityUid uid, string name)
    {
        var storage = Comp<StorageComponent>(uid);
        foreach (var item in storage.Container.ContainedEntities)
        {
            if (Name(item) == name)
                return item;
        }

        return null;
    }

    public EntityUid? FindItem(EntityUid uid, EntityWhitelist? whitelist)
    {
        var storage = Comp<StorageComponent>(uid);
        foreach (var item in storage.Container.ContainedEntities)
        {
            if (_whitelist.IsWhitelistPassOrNull(whitelist, item))
                return item;
        }

        return null;
    }

    public bool GrabItem(Entity<AutodocComponent, HandsComponent> ent, EntityUid item)
    {
        return _hands.TryPickup(ent, item, ent.Comp1.ItemSlot, animate: false, handsComp: ent.Comp2);
    }

    public void GrabItemOrThrow(Entity<AutodocComponent, HandsComponent> ent, EntityUid item)
    {
        if (!GrabItem(ent, item))
            throw new AutodocError("hand-full");
    }

    public void StoreItemOrThrow(Entity<AutodocComponent, HandsComponent> ent)
    {
        var item = GetHeldOrThrow(ent);
        if (!_storage.Insert(ent, item, out _))
            throw new AutodocError("storage-full");
    }

    public EntityUid GetHeldOrThrow(Entity<AutodocComponent, HandsComponent> ent)
    {
        if (!_hands.TryGetHand(ent, ent.Comp1.ItemSlot, out var hand, ent.Comp2))
            throw new AutodocError("item-unavailable");

        if (hand.HeldEntity is not {} item)
            throw new AutodocError("item-unavailable");

        return item;
    }

    public void LabelItem(EntityUid item, string label)
    {
        _label.Label(item, label);
    }

    public void DelayUpdate(EntityUid uid, TimeSpan delay)
    {
        if (TryComp<ActiveAutodocComponent>(uid, out var active))
            active.NextUpdate += delay;
    }

    public EntityUid? GetPatient(Entity<AutodocComponent> ent)
    {
        if (!TryComp<StrapComponent>(ent.Comp.OperatingTable, out var strap))
            return null;

        var buckled = strap.BuckledEntities;
        if (buckled.Count == 0)
            return null;

        var patient = buckled.First();
        if (!HasComp<SurgeryTargetComponent>(patient))
            return null; // TODO: auto draping anything with a body

        return patient;
    }

    public EntityUid GetPatientOrThrow(Entity<AutodocComponent> ent)
    {
        if (GetPatient(ent) is not {} patient)
            throw new AutodocError("missing-patient");

        return patient;
    }

    public EntityUid? FindPart(EntityUid patient, BodyPartType type, BodyPartSymmetry? symmetry)
    {
        foreach (var ent in _body.GetBodyChildrenOfType(patient, type, symmetry: symmetry))
        {
            return ent.Id;
        }

        return null;
    }

    /// <summary>
    /// Starts doing a surgery, returns true if successful.
    /// </summary>
    public bool StartSurgery(Entity<AutodocComponent> ent, EntityUid patient, EntityUid part, EntProtoId surgery)
    {
        if (ent.Comp.RequireSleeping && IsAwake(patient))
            throw new AutodocError("patient-unsedated");

        if (_surgery.GetSingleton(surgery) is not {} singleton)
            return false;

        if (_surgery.GetNextStep(patient, part, singleton) is not {} pair)
            return false;

        var nextSurgery = pair.Item1;
        var index = pair.Item2;
        var nextStep = nextSurgery.Comp.Steps[index];
        if (!_surgery.TryDoSurgeryStep(patient, part, ent, MetaData(nextSurgery).EntityPrototype!.ID, nextStep))
            return false;

        Comp<ActiveAutodocComponent>(ent).CurrentSurgery = (patient, part, surgery);
        return true;
    }

    public bool IsAwake(EntityUid uid)
    {
        return _mobState.IsAlive(uid) && !HasComp<SleepingComponent>(uid);
    }

    /// <summary>
    /// Create a blank program and return the index to it.
    /// Programs cannot be created while operating or if there are too many, in which case it will return null.
    /// </summary>
    public int? CreateProgram(Entity<AutodocComponent> ent, string title)
    {
        var index = ent.Comp.Programs.Count;
        if (IsActive(ent) || index >= ent.Comp.MaxPrograms)
            return null;

        if (string.IsNullOrEmpty(title) || title.Length > ent.Comp.MaxProgramTitleLength)
            return null;

        ent.Comp.Programs.Add(new AutodocProgram()
        {
            Title = title
        });
        Dirty(ent);
        return index;
    }

    /// <summary>
    /// Removes a program at an index, returning true if it succeeded.
    /// </summary>
    public bool RemoveProgram(Entity<AutodocComponent> ent, int index)
    {
        if (IsActive(ent) || index >= ent.Comp.Programs.Count)
            return false;

        ent.Comp.Programs.RemoveAt(index);
        Dirty(ent);
        return true;
    }

    /// <summary>
    /// Adds a step to a program at an index, returning true if it succeeded.
    /// </summary>
    public bool AddStep(Entity<AutodocComponent> ent, int programIndex, IAutodocStep step, int index, EntityUid user)
    {
        if (IsActive(ent) || programIndex >= ent.Comp.Programs.Count)
            return false;

        var program = ent.Comp.Programs[programIndex];
        if (program.Steps.Count >= ent.Comp.MaxProgramSteps || index < 0 || index > program.Steps.Count)
            return false;

        program.Steps.Insert(index, step);
        Dirty(ent);

        _adminLogger.Add(LogType.InteractActivate, LogImpact.Low, $"{ToPrettyString(user):user} added step '{step.Title}' to autodoc program '{program.Title}'");
        return true;
    }

    /// <summary>
    /// Removes a step from a program, returning true if it succeeded.
    /// </summary>
    public bool RemoveStep(Entity<AutodocComponent> ent, int programIndex, int step)
    {
        if (IsActive(ent) || programIndex >= ent.Comp.Programs.Count)
            return false;

        var program = ent.Comp.Programs[programIndex];
        if (step >= program.Steps.Count)
            return false;

        program.Steps.RemoveAt(step);
        Dirty(ent);
        return true;
    }

    public bool IsActive(EntityUid uid)
    {
        return HasComp<ActiveAutodocComponent>(uid);
    }

    public AutodocProgram CurrentProgram(Entity<AutodocComponent, ActiveAutodocComponent> ent)
    {
        // not checking if it exists since Programs isnt allowed to be changed while operating
        return ent.Comp1.Programs[ent.Comp2.CurrentProgram];
    }

    public bool StartProgram(Entity<AutodocComponent> ent, int index, EntityUid user)
    {
        // no error since UI checks this too
        if (IsActive(ent) || index >= ent.Comp.Programs.Count || GetPatient(ent) is not {} patient)
            return false;

        var active = EnsureComp<ActiveAutodocComponent>(ent);
        active.CurrentProgram = index;
        active.NextUpdate = Timing.CurTime + ent.Comp.UpdateDelay;
        Dirty(ent.Owner, active);

        _adminLogger.Add(LogType.InteractActivate, LogImpact.High, $"{ToPrettyString(user):user} started autodoc program '{ent.Comp.Programs[index].Title}' on {ToPrettyString(patient):patient}");
        return true;
    }

    /// <summary>
    /// Tries to start the next step, shouting the error if it fails.
    /// Returns true if the program is being stopped.
    /// </summary>
    public bool Proceed(Entity<AutodocComponent, ActiveAutodocComponent> ent)
    {
        if (ent.Comp2.Waiting)
            return false;

        // stay on this AutodocSurgeryStep until every step of the surgery (and its dependencies) is complete
        // if this was the last step, StartSurgery will fail and the next autodoc step will run
        if (ent.Comp2.CurrentSurgery is {} args)
        {
            var (body, part, surgery) = args;
            if (StartSurgery((ent.Owner, ent.Comp1), body, part, surgery))
            {
                ent.Comp2.Waiting = true;
                return false;
            }

            // done with the surgery onto next step!!!
            ent.Comp2.CurrentSurgery = null;
            ent.Comp2.ProgramStep++;
        }

        var program = ent.Comp1.Programs[ent.Comp2.CurrentProgram];
        var index = ent.Comp2.ProgramStep;
        if (index >= program.Steps.Count)
        {
            Say(ent, Loc.GetString("autodoc-program-completed"));
            return true;
        }

        try
        {
            var step = program.Steps[index];
            if (step.Run((ent.Owner, ent.Comp1, Comp<HandsComponent>(ent)), this))
                ent.Comp2.ProgramStep++;
            else
                ent.Comp2.Waiting = true;
        }
        catch (AutodocError e)
        {
            var error = Loc.GetString("autodoc-error-" + e.Message);
            if (program.SkipFailed)
            {
                Say(ent, Loc.GetString("autodoc-error", ("error", error)));
                ent.Comp2.ProgramStep++;
            }
            else
            {
                Say(ent, Loc.GetString("autodoc-fatal-error", ("error", error)));
                return true;
            }
        }

        Dirty(ent.Owner, ent.Comp1);
        return false;
    }

    #endregion

    public virtual void Say(EntityUid uid, string msg)
    {
    }

    public void SetSafety(Entity<AutodocComponent> ent, bool enabled)
    {
        if (enabled == ent.Comp.RequireSleeping)
            return;

        ent.Comp.RequireSleeping = enabled;
        Dirty(ent);
    }
}

/// <summary>
/// Error autodoc steps can use to abort the program execution and shout an error message.
/// </summary>
public sealed class AutodocError : Exception
{
    /// <summary>
    /// Message has "autodoc-error-" prepended to it, then it gets localized.
    /// </summary>
    public AutodocError(string message) : base(message)
    {
    }
}
