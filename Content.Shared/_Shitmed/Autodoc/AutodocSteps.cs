using Content.Shared._Shitmed.Autodoc.Components;
using Content.Shared._Shitmed.Autodoc.Systems;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.Body.Part;
using Content.Shared.Hands.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Autodoc;

[Serializable, NetSerializable, DataRecord]
public sealed partial class AutodocProgram
{
    public List<IAutodocStep> Steps = new();
    public bool SkipFailed;
    public string Title = string.Empty;
}

/// <summary>
/// Something the autodoc can do during a program.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public partial interface IAutodocStep
{
    /// <summary>
    /// Title of this step to display in the UI
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Run the step, returning true if it is instantly complete and ready to go to the next step, or false if it needs to wait for something else.
    /// Should throw AutodocError for player-facing errors.
    /// </summary>
    bool Run(Entity<AutodocComponent, HandsComponent> ent, SharedAutodocSystem autodoc);

    /// <summary>
    /// Check that this step is valid, returning false if it isn't.
    /// </summary>
    bool Validate(Entity<AutodocComponent> ent, SharedAutodocSystem autodoc)
    {
        return true;
    }
}

/// <summary>
/// Perform a surgery including any prerequesites like opening an incision.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class SurgeryAutodocStep : IAutodocStep
{
    /// <summary>
    /// The type of part to operate on.
    /// </summary>
    [DataField(required: true)]
    public BodyPartType Part;

    /// <summary>
    /// The symmetry required. If this is null then symmetry is not checked (operate on an arbitrary leg for example).
    /// </summary>
    [DataField]
    public BodyPartSymmetry? Symmetry;

    /// <summary>
    /// The ID of the surgery to perform.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId<SurgeryComponent> Surgery;

    public string Title {
        get {
            var protoMan = IoCManager.Resolve<IPrototypeManager>();
            var proto = protoMan.Index(Surgery);
            var part = Loc.GetString("autodoc-body-part-" + Part.ToString());
            return Loc.GetString("autodoc-program-step-surgery", ("part", part), ("name", proto.Name));
        }
    }

    bool IAutodocStep.Run(Entity<AutodocComponent, HandsComponent> ent, SharedAutodocSystem autodoc)
    {
        var patient = autodoc.GetPatientOrThrow((ent.Owner, ent.Comp1));
        if (autodoc.FindPart(patient, Part, Symmetry) is not {} part)
            throw new AutodocError("body-part");

        if (!autodoc.StartSurgery((ent.Owner, ent.Comp1), patient, part, Surgery))
            throw new AutodocError("surgery-impossible");

        return false; // wait for the surgery to be completed before going onto the next program step
    }

    bool IAutodocStep.Validate(Entity<AutodocComponent> ent, SharedAutodocSystem autodoc)
    {
        return autodoc.IsSurgery(Surgery);
    }
}

/// <summary>
/// Grab a specific item from storage, failing if it isn't found.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class GrabItemAutodocStep : IAutodocStep
{
    /// <summary>
    /// The name that an item in storage must match to get grabbed.
    /// </summary>
    [DataField(required: true)]
    public string Name = string.Empty;

    public string Title => Loc.GetString("autodoc-program-step-grab-item", ("name", Name));

    bool IAutodocStep.Validate(Entity<AutodocComponent> ent, SharedAutodocSystem autodoc)
    {
        // client will never send a blank string for name
        return !string.IsNullOrEmpty(Name) && Name.Length <= 100;
    }

    bool IAutodocStep.Run(Entity<AutodocComponent, HandsComponent> ent, SharedAutodocSystem autodoc)
    {
        if (autodoc.FindItem(ent, Name) is not {} item)
            throw new AutodocError("item-unavailable");
        autodoc.GrabItemOrThrow(ent, item);
        return true;
    }
}

/// <summary>
/// Grab the first item that matches a whitelist, failing if none are found.
/// </summary>
[Serializable, NetSerializable]
public abstract partial class GrabAnyItemAutodocStep : IAutodocStep
{
    /// <summary>
    /// A whitelist that must be matched.
    /// </summary>
    public virtual EntityWhitelist Whitelist { get; }
    private EntityWhitelist? _whitelist;

    /// <summary>
    /// Name that represents the whitelist.
    /// </summary>
    public virtual LocId Name { get; }

    string IAutodocStep.Title => Loc.GetString("autodoc-program-step-grab-any", ("name", Loc.GetString(Name)));

    bool IAutodocStep.Run(Entity<AutodocComponent, HandsComponent> ent, SharedAutodocSystem autodoc)
    {
        if (autodoc.FindItem(ent, _whitelist ??= Whitelist) is not {} item)
            throw new AutodocError("item-unavailable");
        autodoc.GrabItemOrThrow(ent, item);
        return true;
    }
}

[Serializable, NetSerializable]
public sealed partial class GrabAnyOrganAutodocStep : GrabAnyItemAutodocStep
{
    public override EntityWhitelist Whitelist => new EntityWhitelist()
    {
        Components = ["Organ"]
    };

    public override LocId Name => "autodoc-item-organ";
}

[Serializable, NetSerializable]
public sealed partial class GrabAnyBodyPartAutodocStep : GrabAnyItemAutodocStep
{
    public override EntityWhitelist Whitelist => new EntityWhitelist()
    {
        Components = ["BodyPart"]
    };

    public override LocId Name => "autodoc-item-part";
}

/// <summary>
/// Store the held item in storage, failing if it can't be picked up.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class StoreItemAutodocStep : IAutodocStep
{
    string IAutodocStep.Title => Loc.GetString("autodoc-program-step-store-item");

    bool IAutodocStep.Run(Entity<AutodocComponent, HandsComponent> ent, SharedAutodocSystem autodoc)
    {
        autodoc.StoreItemOrThrow(ent);
        return true;
    }
}

/// <summary>
/// Gives the held item a label, failing if there is no held item.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class SetLabelAutodocStep : IAutodocStep
{
    [DataField(required: true)]
    public string Label = string.Empty;

    string IAutodocStep.Title => Loc.GetString("autodoc-program-step-set-label", ("label", Label));

    bool IAutodocStep.Validate(Entity<AutodocComponent> ent, SharedAutodocSystem autodoc)
    {
        // client will never send a blank string for label
        return !string.IsNullOrEmpty(Label) && Label.Length <= 20;
    }

    bool IAutodocStep.Run(Entity<AutodocComponent, HandsComponent> ent, SharedAutodocSystem autodoc)
    {
        var item = autodoc.GetHeldOrThrow(ent);
        autodoc.LabelItem(item, Label);
        return true;
    }
}

/// <summary>
/// Waits a number of seconds before going onto the next step.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class WaitAutodocStep : IAutodocStep
{
    [DataField(required: true)]
    public int Length;

    string IAutodocStep.Title => Loc.GetString("autodoc-program-step-wait", ("length", Length));

    bool IAutodocStep.Validate(Entity<AutodocComponent> ent, SharedAutodocSystem autodoc)
    {
        return Length > 0 && Length < 30;
    }

    bool IAutodocStep.Run(Entity<AutodocComponent, HandsComponent> ent, SharedAutodocSystem autodoc)
    {
        autodoc.Say(ent, Loc.GetString("autodoc-waiting"));
        autodoc.DelayUpdate(ent, TimeSpan.FromSeconds(Length));
        return true; // Waiting is for surgery
    }
}
