using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Examine;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Shared._Shitmed.Medical.Surgery.Tools;

/// <summary>
///     Examining a surgical or ghetto tool shows everything it can be used for.
/// </summary>
public sealed class SurgeryToolExamineSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SurgeryToolComponent, GetVerbsEvent<ExamineVerb>>(OnGetVerbs);

        SubscribeLocalEvent<BoneGelComponent, SurgeryToolExaminedEvent>(OnExamined);
        SubscribeLocalEvent<BoneSawComponent, SurgeryToolExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CauteryComponent, SurgeryToolExaminedEvent>(OnExamined);
        SubscribeLocalEvent<HemostatComponent, SurgeryToolExaminedEvent>(OnExamined);
        SubscribeLocalEvent<RetractorComponent, SurgeryToolExaminedEvent>(OnExamined);
        SubscribeLocalEvent<ScalpelComponent, SurgeryToolExaminedEvent>(OnExamined);
        SubscribeLocalEvent<DrillComponent, SurgeryToolExaminedEvent>(OnExamined);
        SubscribeLocalEvent<TendingComponent, SurgeryToolExaminedEvent>(OnExamined);
        SubscribeLocalEvent<TweezersComponent, SurgeryToolExaminedEvent>(OnExamined);

        SubscribeLocalEvent<BodyPartComponent, SurgeryToolExaminedEvent>(OnExamined);
        SubscribeLocalEvent<OrganComponent, SurgeryToolExaminedEvent>(OnExamined);
    }

    private void OnGetVerbs(Entity<SurgeryToolComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var msg = FormattedMessage.FromMarkupOrThrow(Loc.GetString("surgery-tool-header"));
        msg.PushNewline();
        var ev = new SurgeryToolExaminedEvent(msg);
        RaiseLocalEvent(ent, ref ev);

        _examine.AddDetailedExamineVerb(args, ent.Comp, ev.Message,
            Loc.GetString("surgery-tool-examinable-verb-text"), "/Textures/Objects/Specific/Medical/Surgery/scalpel.rsi/scalpel.png",
            Loc.GetString("surgery-tool-examinable-verb-message"));
    }

    private void OnExamined(EntityUid uid, ISurgeryToolComponent comp, ref SurgeryToolExaminedEvent args)
    {
        var msg = args.Message;
        var color = comp.Speed switch
        {
            < 1f => "red",
            > 1f => "green",
            _ => "white"
        };
        var key = "surgery-tool-" + (comp.Used == true ? "used" : "unlimited");
        var speed = comp.Speed.ToString("N2"); // 2 decimal places to not get trolled by float
        msg.PushMarkup(Loc.GetString(key, ("tool", comp.ToolName), ("speed", speed), ("color", color)));
    }
}

[ByRefEvent]
public record struct SurgeryToolExaminedEvent(FormattedMessage Message);
