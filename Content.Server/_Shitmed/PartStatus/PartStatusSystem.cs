// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Text;
using Content.Server.Body.Systems;
using Content.Server.Chat.Managers;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.PartStatus.Events;
using Content.Shared.Body.Part;
using Content.Shared.Chat;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Player;
using Robust.Shared.Utility;

using Content.Goobstation.Common.Examine; // Goobstation Change
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Verbs;
using Robust.Shared.Utility;
using Content.Shared.HealthExaminable;

namespace Content.Server._Shitmed.PartStatus;

public sealed class PartStatusSystem : EntitySystem
{
    [Dependency] private readonly WoundSystem _woundSystem = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;

    private static readonly IReadOnlyList<BodyPartType> BodyPartOrder = new List<BodyPartType>
    {
        BodyPartType.Head,
        BodyPartType.Chest,
        BodyPartType.Arm,
        BodyPartType.Hand,
        BodyPartType.Groin,
        BodyPartType.Leg,
        BodyPartType.Foot,
    }.AsReadOnly();

    private static List<BodyPartSymmetry> _symmetryPriority =
    [
        BodyPartSymmetry.Left,
        BodyPartSymmetry.Right,
        BodyPartSymmetry.None,
    ];

    private const string BleedLocaleStr = "inspect-wound-Bleeding-moderate";
    private const string BoneLocaleStr = "inspect-trauma-BoneDamage";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<GetPartStatusEvent>(OnGetPartStatus);
        SubscribeLocalEvent<HealthExaminableComponent, GetVerbsEvent<ExamineVerb>>(OnGetExamineVerbs);
    }

    private void OnGetPartStatus(GetPartStatusEvent message, EntitySessionEventArgs args)
    {
        var entity = GetEntity(message.Uid);

        if (_mobStateSystem.IsIncapacitated(entity) ||
            !TryComp<ActorComponent>(entity, out var actor) ||
            !_bodySystem.TryGetRootPart(entity, out var rootPart))
            return;

        var partStatusSet = CollectPartStatuses(rootPart.Value);
        var text = GetExamineText(entity, entity, partStatusSet);

        _chat.ChatMessageToOne(
            ChatChannel.Emotes,
            text.ToMarkup(),
            text.ToMarkup(),
            EntityUid.Invalid,
            false,
            actor.PlayerSession.Channel,
            recordReplay: false);
    }


    private void OnGetExamineVerbs(EntityUid uid, HealthExaminableComponent component, GetVerbsEvent<ExamineVerb> args)
    {
        if (!TryComp<DamageableComponent>(uid, out var damage))
            return;

        var detailsRange = _examineSystem.IsInDetailsRange(args.User, uid);

        var verb = new ExamineVerb()
        {
            Act = () =>
            {
                var markup = CreateMarkup(uid, args.User, component, damage);
                _examineSystem.SendExamineTooltip(args.User, uid, markup, false, false);
                var examineCompletedEvent = new ExamineCompletedEvent(markup, uid, args.User, true); // Goobstation
                RaiseLocalEvent(uid, examineCompletedEvent); // Goobstation
            },
            Text = Loc.GetString("health-examinable-verb-text"),
            Category = VerbCategory.Examine,
            Disabled = !detailsRange,
            Message = detailsRange ? null : Loc.GetString("health-examinable-verb-disabled"),
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/rejuvenate.svg.192dpi.png"))
        };

        args.Verbs.Add(verb);
    }

    public FormattedMessage CreateMarkup(EntityUid uid, EntityUid examiner, HealthExaminableComponent component, DamageableComponent damage)
    {
        if (!_bodySystem.TryGetRootPart(uid, out var rootPart))
            return new FormattedMessage();

        var partStatusSet = CollectPartStatuses(rootPart.Value);
        var text = GetExamineText(uid, examiner, partStatusSet, false);
        // Anything else want to add on to this?
        RaiseLocalEvent(uid, new HealthBeingExaminedEvent(text), true);

        return text;
    }


    private HashSet<PartStatus> CollectPartStatuses(Entity<BodyPartComponent> rootPart)
    {
        var partStatusSet = new HashSet<PartStatus>();

        foreach (var woundable in _woundSystem.GetAllWoundableChildren(rootPart))
        {
            if (!TryComp<BodyPartComponent>(woundable, out var bodyPartComponent) ||
                !TryComp<BoneComponent>(woundable.Comp.Bone.ContainedEntities.FirstOrNull(), out var bone))
                continue;

            var partName = bodyPartComponent.ParentSlot?.Id ?? bodyPartComponent.PartType.ToString().ToLower();
            var (damageSeverities, isBleeding) = AnalyzeWounds(woundable);

            partStatusSet.Add(new PartStatus(
                bodyPartComponent.PartType,
                bodyPartComponent.Symmetry,
                partName,
                woundable.Comp.WoundableSeverity,
                damageSeverities,
                bone.BoneSeverity,
                isBleeding));
        }

        return partStatusSet;
    }

    private (Dictionary<string, WoundSeverity> DamageSeverities, bool IsBleeding) AnalyzeWounds(
        Entity<WoundableComponent> woundable)
    {
        var damageSeverities = new Dictionary<string, WoundSeverity>();
        var isBleeding = false;

        foreach (var wound in _woundSystem.GetWoundableWounds(woundable))
        {
            if (wound.Comp.DamageGroup == null
                || wound.Comp.WoundSeverity == WoundSeverity.Healed)
                continue;

            if (!damageSeverities.TryGetValue(wound.Comp.DamageType, out var existingSeverity) ||
                wound.Comp.WoundSeverity > existingSeverity)
                damageSeverities[wound.Comp.DamageGroup.LocalizedName] = wound.Comp.WoundSeverity;

            if (TryComp<BleedInflicterComponent>(wound, out var bleeds) && bleeds.IsBleeding)
                isBleeding = true;
        }

        return (damageSeverities, isBleeding);
    }

    private FormattedMessage GetExamineText(EntityUid entity,
        EntityUid examiner,
        HashSet<PartStatus> partStatusSet,
        bool styling = true)
    {
        var message = new FormattedMessage();
        var titlestring = entity == examiner
            ? "inspect-part-status-title"
            : "inspect-part-status-title-other";

        if (styling)
        {
            message.PushTag(new MarkupNode("examineborder", null, null)); // border
            message.PushNewline();
        }
        else
        {
            titlestring += "-styleless";
        }

        message.AddText(Loc.GetString(titlestring, ("entity", Identity.Name(entity, EntityManager))));
        message.PushNewline();
        AddLine(message);
        CreateBodyPartMessage(partStatusSet, entity == examiner, ref message, !styling);

        if (styling)
        {
            message.Pop();
            message.PushNewline();
        }

        return message;
    }

    private void CreateBodyPartMessage(HashSet<PartStatus> partStatusSet,
        bool inspectingSelf,
        ref FormattedMessage message,
        bool styleless = false)
    {
        var orderedParts = BodyPartOrder
            .SelectMany(partType => partStatusSet.Where(p => p.PartType == partType)
                .ToList()
                .OrderBy(p => _symmetryPriority.IndexOf(p.PartSymmetry)))
            .ToList();

        foreach (var partStatus in orderedParts)
        {
            var statusDescription = BuildStatusDescription(partStatus, inspectingSelf);
            var possessive = inspectingSelf
                ? Loc.GetString("inspect-part-status-you")
                : Loc.GetString("inspect-part-status-their");

            var locString = "inspect-part-status-line";

            if (styleless)
            {
                locString += "-styleless";
            }

            message.AddText("    " + Loc.GetString(locString,
                ("possessive", possessive),
                ("part", partStatus.PartName),
                ("status", statusDescription)));

            message.PushNewline();
        }
    }

    private string BuildStatusDescription(PartStatus partStatus, bool inspectingSelf)
    {
        var sb = new StringBuilder();
        var hasStatus = false;

        AppendBleedingStatus(sb, partStatus.Bleeding, inspectingSelf, ref hasStatus);
        AppendBoneStatus(sb, partStatus.BoneSeverity, inspectingSelf, ref hasStatus);
        AppendDamageStatuses(sb, partStatus.DamageSeverities, inspectingSelf, ref hasStatus);

        if (!hasStatus)
            sb.Append(Loc.GetString("inspect-part-status-fine"));

        return sb.ToString();
    }

    private void AppendBleedingStatus(StringBuilder sb, bool isBleeding, bool inspectingSelf, ref bool hasStatus)
    {
        if (!isBleeding)
            return;

        sb.Append(Loc.GetString(inspectingSelf ? $"self-{BleedLocaleStr}" : BleedLocaleStr));
        hasStatus = true;
    }

    private void AppendBoneStatus(StringBuilder sb, BoneSeverity boneSeverity, bool inspectingSelf, ref bool hasStatus)
    {
        if (boneSeverity <= BoneSeverity.Normal)
            return;

        if (hasStatus)
            sb.Append($"{Loc.GetString("inspect-part-status-comma")} ");


        sb.Append(Loc.GetString(inspectingSelf ? $"self-{BoneLocaleStr}" : BoneLocaleStr));
        hasStatus = true;
    }

    private void AppendDamageStatuses(
        StringBuilder sb,
        Dictionary<string, WoundSeverity> damageSeverities,
        bool inspectingSelf,
        ref bool hasStatus)
    {
        foreach (var (type, severity) in damageSeverities)
        {
            if (type is not ("Brute" or "Burn"))
                continue;

            if (hasStatus)
                sb.Append(!type.Contains(Loc.GetString("inspect-part-status-conjunction"), StringComparison.CurrentCultureIgnoreCase) ? $" {Loc.GetString("inspect-part-status-conjunction")} " : $"{Loc.GetString("inspect-part-status-comma")} ");

            var cappedSeverity = severity > WoundSeverity.Severe ? WoundSeverity.Severe : severity;
            var localeText = $"inspect-wound-{type}-{cappedSeverity.ToString().ToLower()}";
            sb.Append(Loc.GetString(inspectingSelf ? $"self-{localeText}" : localeText));
            hasStatus = true;
        }
    }

    private void AddLine(FormattedMessage message)
    {
        message.PushColor(Color.FromHex("#282D31"));
        message.AddText(Loc.GetString("examine-border-line"));
        message.PushNewline();
        message.Pop();
    }
}
