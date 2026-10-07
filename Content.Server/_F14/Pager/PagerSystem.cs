using System.Linq;
using Content.Shared._F14.Pager;
using Content.Shared._F14.SCPOS;
using Content.Shared.Access.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Server.Access.Systems;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Radiation.Components;
using Content.Server._Pirate.Banking;
using Content.Shared._Pirate.Banking.Components;
using Content.Shared._Pirate.Banking;
using Content.Server.Chat.Managers;
using Content.Shared.Chat;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using Content.Server.Chat.Systems;

namespace Content.Server._F14.Pager;

public sealed class PagerSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IdCardSystem _idCardSystem = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly BankCardSystem _bankCardSystem = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PagerComponent, PagerButtonPressedMessage>(OnButtonPressed);
        SubscribeLocalEvent<PagerComponent, PagerSendMessageAlert>(OnSendMessage);
        SubscribeLocalEvent<PagerComponent, PagerCallRequestMessage>(OnCallRequest);
        SubscribeLocalEvent<PagerComponent, PagerDownloadSoftwareMessage>(OnDownloadSoftware);
        SubscribeLocalEvent<PagerComponent, PagerUninstallSoftwareMessage>(OnUninstallSoftware);
        SubscribeLocalEvent<PagerComponent, PagerEjectIdMessage>(OnEjectId);
        SubscribeLocalEvent<PagerComponent, PagerAddNoteMessage>(OnAddNote);
        SubscribeLocalEvent<PagerComponent, PagerDeleteNoteMessage>(OnDeleteNote);
        SubscribeLocalEvent<PagerComponent, PagerCycleShieldsMessage>(OnCycleShields);
        SubscribeLocalEvent<PagerComponent, PagerOrderSupplyMessage>(OnOrderSupply);
        SubscribeLocalEvent<PagerComponent, PagerTriggerScanMessage>(OnTriggerScan);
        SubscribeLocalEvent<PagerComponent, PagerSetIdInfoMessage>(OnSetIdInfo);
        SubscribeLocalEvent<PagerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<PagerComponent, EntInsertedIntoContainerMessage>(OnIdCardInserted);
        SubscribeLocalEvent<PagerComponent, EntRemovedFromContainerMessage>(OnIdCardRemoved);
        SubscribeLocalEvent<InventoryComponent, EntitySpokeEvent>(OnEntitySpoke);
    }

    private void OnStartup(EntityUid uid, PagerComponent comp, ComponentStartup args)
    {
        _itemSlots.AddItemSlot(uid, PagerComponent.IdCardSlotId, new ItemSlot());
    }

    private void OnButtonPressed(EntityUid uid, PagerComponent comp, PagerButtonPressedMessage args)
    {
        if (comp.ClickSound != null && args.Button != PagerButton.WheelUp && args.Button != PagerButton.WheelDown)
            _audio.PlayPvs(comp.ClickSound, uid);

        switch (args.Button)
        {
            case PagerButton.Green:
                if (comp.CallState == PagerCallState.IncomingRinging && comp.ActiveCallTarget != null)
                {
                    AcceptCall(uid, comp, comp.ActiveCallTarget.Value);
                }
                break;

            case PagerButton.Red:
                if (comp.CallState != PagerCallState.Idle)
                {
                    EndCall(uid, comp);
                }
                break;
        }

        UpdateUI(uid, comp);
    }

    private void OnOrderSupply(EntityUid uid, PagerComponent comp, PagerOrderSupplyMessage args)
    {
        var catalog = GetSupplyCatalog();
        var item = catalog.FirstOrDefault(i => i.Id == args.ItemId);
        var user = Transform(uid).ParentUid;

        if (string.IsNullOrEmpty(item.Id))
            return;

        if (comp.SupplyBudget < item.Cost)
        {
            _popup.PopupEntity("REQUISITION ERROR: Insufficient logistics budget.", uid, user);
            return;
        }

        comp.SupplyBudget -= item.Cost;
        _popup.PopupEntity($"ORDER DISPATCHED: {item.Name} scheduled for cargo drop.", uid, user);
        UpdateUI(uid, comp);
    }

    private void OnTriggerScan(EntityUid uid, PagerComponent comp, PagerTriggerScanMessage args)
    {
        var user = Transform(uid).ParentUid;
        _popup.PopupEntity("SCAN COMPLETE: Environment telemetry updated.", uid, user);
        UpdateUI(uid, comp);
    }

    private void OnCycleShields(EntityUid uid, PagerComponent comp, PagerCycleShieldsMessage args)
    {
        var user = Transform(uid).ParentUid;
        _popup.PopupEntity("SHIELD TELEMETRY: Harmonics re-calibrated.", uid, user);
        UpdateUI(uid, comp);
    }

    private void OnAddNote(EntityUid uid, PagerComponent comp, PagerAddNoteMessage args)
    {
        if (string.IsNullOrWhiteSpace(args.Text))
            return;

        comp.Notes.Add(args.Text.Trim());
        UpdateUI(uid, comp);
    }

    private void OnDeleteNote(EntityUid uid, PagerComponent comp, PagerDeleteNoteMessage args)
    {
        if (args.Index >= 0 && args.Index < comp.Notes.Count)
        {
            comp.Notes.RemoveAt(args.Index);
            UpdateUI(uid, comp);
        }
    }

    private void OnCallRequest(EntityUid uid, PagerComponent comp, PagerCallRequestMessage args)
    {
        var targetPager = GetEntity(args.TargetPager);

        if (targetPager == uid || !TryComp<PagerComponent>(targetPager, out var targetComp))
            return;

        var callerUser = GetUserHoldingPager(uid);
        var targetUser = GetUserHoldingPager(targetPager);

        if (callerUser == null || !HasEarpieceEquipped(callerUser.Value))
        {
            _popup.PopupEntity("You need an active earpiece equipped in your ear slot to initiate a call!", uid, callerUser ?? uid);
            return;
        }

        if (targetUser == null || !HasEarpieceEquipped(targetUser.Value))
        {
            _popup.PopupEntity("The recipient has no active earpiece connected!", uid, callerUser.Value);
            return;
        }

        comp.CallState = PagerCallState.OutgoingRinging;
        comp.ActiveCallTarget = targetPager;

        targetComp.CallState = PagerCallState.IncomingRinging;
        targetComp.ActiveCallTarget = uid;

        PlayRingSequence(targetPager, targetComp);

        UpdateUI(uid, comp);
        UpdateUI(targetPager, targetComp);
    }

    private void PlayRingSequence(EntityUid uid, PagerComponent comp, int repetitions = 4, float interval = 0.8f)
    {
        if (comp.RingSound == null)
            return;

        for (var i = 0; i < repetitions; i++)
        {
            var delay = i * interval;
            Timer.Spawn(TimeSpan.FromSeconds(delay), () =>
            {
                if (Deleted(uid) || !TryComp<PagerComponent>(uid, out var currentComp))
                    return;

                if (currentComp.CallState == PagerCallState.IncomingRinging && currentComp.RingSound != null)
                {
                    _audio.PlayPvs(currentComp.RingSound, uid);
                }
            });
        }
    }

    private void AcceptCall(EntityUid receiver, PagerComponent receiverComp, EntityUid caller)
    {
        if (!TryComp<PagerComponent>(caller, out var callerComp))
            return;

        receiverComp.CallState = PagerCallState.InCall;
        callerComp.CallState = PagerCallState.InCall;

        UpdateUI(receiver, receiverComp);
        UpdateUI(caller, callerComp);
    }

    private void EndCall(EntityUid uid, PagerComponent comp)
    {
        if (comp.ActiveCallTarget is { } target && TryComp<PagerComponent>(target, out var targetComp))
        {
            targetComp.CallState = PagerCallState.Idle;
            targetComp.ActiveCallTarget = null;
            UpdateUI(target, targetComp);
        }

        comp.CallState = PagerCallState.Idle;
        comp.ActiveCallTarget = null;
        UpdateUI(uid, comp);
    }

    private void OnEntitySpoke(EntityUid uid, InventoryComponent component, EntitySpokeEvent args)
    {
        if (string.IsNullOrWhiteSpace(args.Message))
            return;

        var speaker = uid;

        if (!TryGetActiveCall(speaker, out var callerPager, out var callerComp, out var targetPager, out var targetComp))
            return;

        if (!HasEarpieceEquipped(speaker))
            return;

        var targetUser = GetUserHoldingPager(targetPager);
        if (targetUser == null || !HasEarpieceEquipped(targetUser.Value))
            return;

        var speakerName = !string.IsNullOrWhiteSpace(callerComp.OwnerName) && callerComp.OwnerName != "Unknown Operator"
            ? callerComp.OwnerName
            : Identity.Name(speaker, EntityManager);

        var targetName = !string.IsNullOrWhiteSpace(targetComp.OwnerName) && targetComp.OwnerName != "Unknown Operator"
            ? targetComp.OwnerName
            : Identity.Name(targetUser.Value, EntityManager);

        var safeSpeaker = FormattedMessage.EscapeText(speakerName);
        var safeTarget = FormattedMessage.EscapeText(targetName);
        var safeMessage = FormattedMessage.EscapeText(args.Message);

        var recipientMsg = $"[color=#526c48](Call: {safeSpeaker})[/color] says, \"{safeMessage}\"";
        var callerFeedback = $"[color=#526c48](Call -> {safeTarget})[/color] says, \"{safeMessage}\"";

        if (TryComp<ActorComponent>(targetUser.Value, out var targetActor))
        {
            _chatManager.ChatMessageToOne(
                ChatChannel.Radio,
                args.Message,
                recipientMsg,
                targetPager,
                false,
                targetActor.PlayerSession.Channel);
        }

        if (TryComp<ActorComponent>(speaker, out var speakerActor))
        {
            _chatManager.ChatMessageToOne(
                ChatChannel.Radio,
                args.Message,
                callerFeedback,
                callerPager,
                false,
                speakerActor.PlayerSession.Channel);
        }

        if (targetComp.ClickSound != null)
            _audio.PlayPvs(targetComp.ClickSound, targetPager);

        if (callerComp.ClickSound != null)
            _audio.PlayPvs(callerComp.ClickSound, callerPager);
    }

    private EntityUid? GetUserHoldingPager(EntityUid pager)
    {
        var current = Transform(pager).ParentUid;
        var depth = 0;
        while (current.IsValid() && depth < 8)
        {
            if (HasComp<ActorComponent>(current) || HasComp<InventoryComponent>(current))
                return current;

            current = Transform(current).ParentUid;
            depth++;
        }
        return null;
    }

    private bool TryGetActiveCall(
        EntityUid speaker,
        out EntityUid callerPager,
        out PagerComponent callerComp,
        out EntityUid targetPager,
        out PagerComponent targetComp)
    {
        callerPager = default;
        callerComp = null!;
        targetPager = default;
        targetComp = null!;

        var query = EntityQueryEnumerator<PagerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.CallState != PagerCallState.InCall || comp.ActiveCallTarget is not { } targetUid)
                continue;

            if (GetUserHoldingPager(uid) == speaker)
            {
                if (TryComp<PagerComponent>(targetUid, out var tComp) && tComp.CallState == PagerCallState.InCall)
                {
                    callerPager = uid;
                    callerComp = comp;
                    targetPager = targetUid;
                    targetComp = tComp;
                    return true;
                }
            }
        }

        return false;
    }

    private void OnSendMessage(EntityUid uid, PagerComponent comp, PagerSendMessageAlert args)
    {
        if (string.IsNullOrWhiteSpace(args.Content))
            return;

        var msg = new PagerMessage(comp.OwnerName, args.Recipient, args.Content, _timing.CurTime);

        if (string.IsNullOrEmpty(args.Recipient))
        {
            var query = EntityQueryEnumerator<PagerComponent>();
            while (query.MoveNext(out var otherUid, out var otherComp))
            {
                otherComp.Messages.Add(msg);
                if (otherUid != uid && otherComp.RingSound != null)
                    _audio.PlayPvs(otherComp.RingSound, otherUid);

                UpdateUI(otherUid, otherComp);
            }
        }
        else
        {
            comp.Messages.Add(msg);
            UpdateUI(uid, comp);

            var query = EntityQueryEnumerator<PagerComponent>();
            while (query.MoveNext(out var targetUid, out var targetComp))
            {
                if (targetUid != uid && targetComp.OwnerName.Equals(args.Recipient, StringComparison.OrdinalIgnoreCase))
                {
                    targetComp.Messages.Add(msg);
                    if (targetComp.RingSound != null)
                        _audio.PlayPvs(targetComp.RingSound, targetUid);

                    UpdateUI(targetUid, targetComp);
                    break;
                }
            }
        }
    }

    private bool HasEarpieceEquipped(EntityUid user)
    {
        if (!_inventory.TryGetSlotEntity(user, "ears", out var item) || item is not { } earpiece)
            return false;

        var protoId = MetaData(earpiece).EntityPrototype?.ID;

        return HasComp<PagerEarpieceComponent>(earpiece) ||
               (protoId != null && (protoId.Contains("Headset", StringComparison.OrdinalIgnoreCase) ||
                                    protoId.Contains("Earpiece", StringComparison.OrdinalIgnoreCase)));
    }

    private void OnSetIdInfo(EntityUid uid, PagerComponent comp, PagerSetIdInfoMessage args)
    {
        if (!_itemSlots.TryGetSlot(uid, PagerComponent.IdCardSlotId, out var slot) || slot.Item is not { } idCardUid)
            return;

        if (!TryComp<IdCardComponent>(idCardUid, out var idCard))
            return;

        var user = Transform(uid).ParentUid;

        if (!string.IsNullOrWhiteSpace(args.Name))
            _idCardSystem.TryChangeFullName(idCardUid, args.Name.Trim(), idCard);

        if (!string.IsNullOrWhiteSpace(args.JobTitle))
            _idCardSystem.TryChangeJobTitle(idCardUid, args.JobTitle.Trim(), idCard);

        _popup.PopupEntity("ID CARD PROGRAMMED SUCCESSFULLY.", uid, user);
        UpdateUI(uid, comp);
    }

    private void OnIdCardInserted(EntityUid uid, PagerComponent comp, EntInsertedIntoContainerMessage args)
    {
        UpdateUI(uid, comp);
    }

    private void OnIdCardRemoved(EntityUid uid, PagerComponent comp, EntRemovedFromContainerMessage args)
    {
        UpdateUI(uid, comp);
    }

    private List<string> GetCurrentAccessTags(EntityUid pager)
    {
        var tags = new List<string>();

        if (!_itemSlots.TryGetSlot(pager, PagerComponent.IdCardSlotId, out var slot) || slot.Item is not { } idCardUid)
            return tags;

        if (TryComp<AccessComponent>(idCardUid, out var access))
            tags.AddRange(access.Tags.Select(t => t.Id));

        return tags;
    }

    private string? GetInsertedIdName(EntityUid pager)
    {
        if (!_itemSlots.TryGetSlot(pager, PagerComponent.IdCardSlotId, out var slot) || slot.Item is not { } idCardUid)
            return null;

        return Identity.Name(idCardUid, EntityManager);
    }

    private (int? balance, string? account) GetBankData(EntityUid pager)
    {
        var user = Transform(pager).ParentUid;

        if (!_idCardSystem.TryFindIdCard(user, out var idCard))
            return (null, null);

        if (!TryComp<BankCardComponent>(idCard.Owner, out var bankCard) || bankCard.AccountId is not { } accountId)
            return (null, null);

        if (!_bankCardSystem.TryGetAccount(accountId, out var account))
            return (null, null);

        return (account.Balance, account.AccountId.ToString());
    }

    private List<PagerScpEntry> GetScpDossiers()
    {
        return new List<PagerScpEntry>
        {
            new("SCP-173", "The Sculpture", "EUCLID",
                "Keep 3 personnel minimum. Maintain unbroken eye contact until chamber sealed.",
                "Constructed of concrete and rebar. Extremely hostile, attacks via neck snap when line of sight is broken."),
            new("SCP-096", "The Shy Guy", "EUCLID",
                "Enclosed in 5m cube hermetic steel cell. Zero direct or photographic observation permitted.",
                "Docile humanoid. Viewing its face triggers unstoppable homicidal rage until the viewer is eliminated."),
            new("SCP-049", "Plague Doctor", "EUCLID",
                "Standard humanoid containment with Class-B chemical restraint protocol.",
                "Humanoid in 15th-century plague doctor attire. Touch is instantly lethal; attempts crude surgeries to cure 'The Great Pestilence'."),
            new("SCP-106", "The Old Man", "KETER",
                "Lead-lined steel cell suspended in liquid matrix. Dual electro-magnetic field active.",
                "Decomposed humanoid able to corrode solid matter and pull victims into an extra-dimensional pocket dimension."),
            new("SCP-914", "The Clockworks", "SAFE",
                "Research Lab 17-B. Requires Level-2 research authorization.",
                "Massive mechanical clockwork refining device with settings: Rough, Coarse, 1:1, Fine, Very Fine.")
        };
    }

    private List<PagerShieldEntry> GetShieldStatus()
    {
        return new List<PagerShieldEntry>
        {
            new("PRIMARY FACILITY GRID", 100, 420, "ONLINE"),
            new("HEAVY CONTAINMENT VAULT", 86, 780, "ONLINE"),
            new("SECTOR-4 SUB-LEVEL", 0, 0, "BREACH"),
            new("SURFACE RADAR ARRAY", 94, 310, "ONLINE")
        };
    }

    private List<PagerCrewRecord> GetCrewRecords()
    {
        return new List<PagerCrewRecord>
        {
            new("Anderson Shmidt", "LEVEL 4", "ACTIVE", "Disciplinary log clean. Authorized for Site-14 command directives."),
            new("Marcus Vance", "LEVEL 2", "ON PROBATION", "Cited for failure to log Class-3 testing protocol on 2012-04-12."),
            new("Elena Rostova", "LEVEL 3", "ACTIVE", "Cleared medical evaluation. Supervised SCP-049 observation routine."),
            new("D-44819", "LEVEL 0", "DETAINED", "Non-compliant during biological chamber inspection.")
        };
    }

    private List<PagerSupplyItem> GetSupplyCatalog()
    {
        return new List<PagerSupplyItem>
        {
            new("med_trauma", "Trauma First Aid Kit", 180),
            new("ammo_9mm", "9x19mm FMJ Crate (120 rnd)", 240),
            new("ammo_shotgun", "12g Buckshot Crate (60 rnd)", 260),
            new("amnestic_a", "Class-A Amnestic Injector", 450),
            new("rations_mre", "Arctic Field Rations (x4)", 80),
            new("hazmat_gear", "NBC Containment Suit", 320)
        };
    }

    private PagerScannerData GetScannerReadings(EntityUid pager)
    {
        var user = Transform(pager).ParentUid;

        var mixture = _atmosphere.GetTileMixture((user, null));
        var tempC = mixture is { } mix ? mix.Temperature - 273.15f : -273.15f;

        var rads = TryComp<RadiationReceiverComponent>(user, out var receiver)
            ? receiver.CurrentRadiation
            : 0f;

        var hume = 0.97f + (float) _random.NextDouble() * 0.06f;
        var pulse = _random.Next(68, 86);

        var status = rads switch
        {
            > 5f => "RADIATION HAZARD",
            > 1f => "ELEVATED RADIATION",
            _ when tempC < -10f => "EXTREME COLD",
            _ when tempC > 45f => "EXTREME HEAT",
            _ => "ENVIRONMENT STABLE"
        };

        return new PagerScannerData(hume, tempC, rads, pulse, status);
    }

    private int GetDiskUsed(PagerComponent comp)
    {
        var used = 0;
        foreach (var id in comp.InstalledSoftware)
        {
            if (_proto.TryIndex<SCPOSSoftwarePrototype>(id, out var software))
                used += software.DiskCost;
        }

        return used;
    }

    private void OnDownloadSoftware(EntityUid uid, PagerComponent comp, PagerDownloadSoftwareMessage args)
    {
        if (!_proto.TryIndex<SCPOSSoftwarePrototype>(args.SoftwareId, out var software))
            return;

        if (comp.InstalledSoftware.Contains(software.ID))
            return;

        var diskUsed = GetDiskUsed(comp);

        if (diskUsed + software.DiskCost > comp.DiskCapacity)
        {
            if (args.Actor is { } actorNoSpace)
                _popup.PopupEntity("ERROR: Insufficient memory.", uid, actorNoSpace);
            return;
        }

        if (software.RequiredAccess.Count > 0)
        {
            var currentTags = GetCurrentAccessTags(uid);
            var hasAccess = software.RequiredAccess.Any(req => currentTags.Contains(req));

            if (!hasAccess)
            {
                if (args.Actor is { } actorNoAccess)
                    _popup.PopupEntity("ERROR: Access denied — clearance missing on ID card.", uid, actorNoAccess);
                return;
            }
        }

        comp.InstalledSoftware.Add(software.ID);
        UpdateUI(uid, comp);
    }

    private void OnUninstallSoftware(EntityUid uid, PagerComponent comp, PagerUninstallSoftwareMessage args)
    {
        if (!_proto.TryIndex<SCPOSSoftwarePrototype>(args.SoftwareId, out var software) || software.Core)
            return;

        comp.InstalledSoftware.Remove(software.ID);
        UpdateUI(uid, comp);
    }

    private void OnEjectId(EntityUid uid, PagerComponent comp, PagerEjectIdMessage args)
    {
        if (args.Actor is { } actor && _itemSlots.TryGetSlot(uid, PagerComponent.IdCardSlotId, out var slot))
            _itemSlots.TryEjectToHands(uid, slot, actor);

        UpdateUI(uid, comp);
    }

    public void UpdateUI(EntityUid uid, PagerComponent comp)
    {
        var contacts = new List<(NetEntity, string)>();
        var query = EntityQueryEnumerator<PagerComponent>();
        while (query.MoveNext(out var otherUid, out var otherComp))
        {
            if (otherUid != uid)
                contacts.Add((GetNetEntity(otherUid), otherComp.OwnerName));
        }

        string? partnerName = null;
        if (comp.ActiveCallTarget is { } target && TryComp<PagerComponent>(target, out var targetComp))
            partnerName = targetComp.OwnerName;

        var manifest = new List<PagerCrewEntry>();
        var idQuery = EntityQueryEnumerator<IdCardComponent>();
        var seenNames = new HashSet<string>();

        while (idQuery.MoveNext(out var idUid, out var idCard))
        {
            if (string.IsNullOrWhiteSpace(idCard.FullName))
                continue;

            if (!seenNames.Add(idCard.FullName))
                continue;

            string job = "Personnel";

            if (idCard.JobTitle != null && Loc.TryGetString(idCard.JobTitle.Value, out var localizedJob))
            {
                job = localizedJob;
            }
            else if (idCard.JobTitle != null && !string.IsNullOrWhiteSpace(idCard.JobTitle.Value))
            {
                job = idCard.JobTitle.Value;
            }
            else
            {
                var cardName = MetaData(idUid).EntityName;
                var openParen = cardName.LastIndexOf('(');
                var closeParen = cardName.LastIndexOf(')');
                if (openParen != -1 && closeParen > openParen)
                {
                    var inside = cardName.Substring(openParen + 1, closeParen - openParen - 1).Trim();
                    if (!inside.Equals("Nanotrasen", StringComparison.OrdinalIgnoreCase) &&
                        !inside.Equals("Foundation", StringComparison.OrdinalIgnoreCase))
                    {
                        job = inside;
                    }
                }
            }

            manifest.Add(new PagerCrewEntry(idCard.FullName, job));
        }
        manifest.Sort((a, b) => string.Compare(a.JobTitle, b.JobTitle, StringComparison.OrdinalIgnoreCase));

        var (bankBal, bankAcc) = GetBankData(uid);
        var holder = GetUserHoldingPager(uid);
        var earpieceLinked = holder != null && HasEarpieceEquipped(holder.Value);

        var state = new PagerBoundUserInterfaceState(
            comp.OwnerName,
            comp.CallState,
            partnerName,
            comp.Messages,
            contacts,
            manifest,
            GetDiskUsed(comp),
            comp.DiskCapacity,
            comp.InstalledSoftware,
            GetCurrentAccessTags(uid),
            GetInsertedIdName(uid),
            comp.Notes,
            bankBal,
            bankAcc,
            GetScpDossiers(),
            GetShieldStatus(),
            GetCrewRecords(),
            GetSupplyCatalog(),
            comp.SupplyBudget,
            GetScannerReadings(uid),
            earpieceLinked);

        _ui.SetUiState(uid, PagerUiKey.Key, state);
    }
}
