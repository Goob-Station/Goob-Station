using Content.Goobstation.Common.Traitor;
using Content.Goobstation.Common.Traitor.PenSpin;
using Content.Goobstation.Server.Traitor.PenSpin;
using Content.Goobstation.Shared.Traitor.PenSpin;
using Content.Server.Preferences.Managers;
using Content.Server.Store.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.PDA;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Store;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Traitor;

public sealed class GoobUplinkSystem : GoobCommonUplinkSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;

    private static readonly ProtoId<TagPrototype> PenTag = "Pen";

    private static readonly ProtoId<RoleLoadoutPrototype> AntagTraitorLoadout = "AntagTraitor";
    private static readonly ProtoId<LoadoutGroupPrototype> TraitorUplinkGroup = "TraitorUplink";
    private static readonly ProtoId<LoadoutPrototype> TraitorUplinkPDA = "TraitorUplinkPDA";
    private static readonly ProtoId<LoadoutPrototype> TraitorUplinkPen = "TraitorUplinkPen";
    private static readonly ProtoId<LoadoutPrototype> TraitorUplinkImplant = "TraitorUplinkImplant";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PenSpinUplinkComponent, CurrencyInsertAttemptEvent>(OnCurrencyInsert);
        SubscribeLocalEvent<PenSpinUplinkComponent, BoundUIClosedEvent>(OnStoreClosed);
        SubscribeLocalEvent<PenSpinComponent, PenSpinSubmitDegreeMessage>(OnSubmitDegree);
        SubscribeLocalEvent<PenSpinComponent, PenSpinResetMessage>(OnReset);
        SubscribeLocalEvent<PenSpinComponent, GeneratePenSpinCodeEvent>(OnGenerateCode);
    }

    public override EntityUid? FindPenUplinkTarget(EntityUid user)
    {
        if (_inventorySystem.TryGetContainerSlotEnumerator(user, out var containerSlotEnumerator))
        {
            while (containerSlotEnumerator.MoveNext(out var slot))
            {
                if (!slot.ContainedEntity.HasValue)
                    continue;

                var item = slot.ContainedEntity.Value;

                if (_tag.HasTag(item, PenTag))
                    return item;

                if (TryComp<PdaComponent>(item, out _))
                {
                    var penInPda = _itemSlots.GetItemOrNull(item, PdaComponent.PdaPenSlotId);
                    if (penInPda != null && _tag.HasTag(penInPda.Value, PenTag))
                        return penInPda.Value;
                }
            }
        }

        foreach (var item in _handsSystem.EnumerateHeld(user))
        {
            if (_tag.HasTag(item, PenTag))
                return item;

            if (TryComp<PdaComponent>(item, out _))
            {
                var penInPda = _itemSlots.GetItemOrNull(item, PdaComponent.PdaPenSlotId);
                if (penInPda != null && _tag.HasTag(penInPda.Value, PenTag))
                    return penInPda.Value;
            }
        }

        return null;
    }

    private void OnGenerateCode(Entity<PenSpinComponent> ent, ref GeneratePenSpinCodeEvent ev)
    {
        var code = new int[ent.Comp.CombinationLength];
        for (var i = 0; i < ent.Comp.CombinationLength; i++)
        {
            code[i] = _random.Next(ent.Comp.MinDegree, ent.Comp.MaxDegree + 1);
        }

        if (TryComp<PenSpinUplinkComponent>(ent.Owner, out var uplink))
        {
            uplink.Code = code;
        }

        ev.Code = code;
    }

    private void OnCurrencyInsert(Entity<PenSpinUplinkComponent> ent, ref CurrencyInsertAttemptEvent args)
    {
        if (!ent.Comp.Unlocked)
            args.Cancel();
    }

    private void OnStoreClosed(Entity<PenSpinUplinkComponent> ent, ref BoundUIClosedEvent args)
    {
        if (args.UiKey is StoreUiKey)
            ent.Comp.Unlocked = false;
    }

    private void OnSubmitDegree(Entity<PenSpinComponent> ent, ref PenSpinSubmitDegreeMessage args)
    {
        if (!IsValidDegree(ent.Comp, args.Degree))
            return;

        if (!TryComp<PenSpinUplinkComponent>(ent, out var uplink))
            return;

        var curTime = _timing.CurTime;
        if (uplink.NextSpinTime.HasValue && curTime < uplink.NextSpinTime.Value)
            return;

        uplink.NextSpinTime = curTime + ent.Comp.SpinCooldown;

        if (uplink.CurrentCombination.Length != ent.Comp.CombinationLength)
            uplink.CurrentCombination = new int[ent.Comp.CombinationLength];

        uplink.CurrentCombination[uplink.CurrentIndex] = args.Degree;

        var hasCode = uplink.Code is not null;
        var isCorrect = hasCode && args.Degree == uplink.Code![uplink.CurrentIndex];

        if (isCorrect)
        {
            uplink.CurrentIndex++;

            if (uplink.CurrentIndex >= ent.Comp.CombinationLength)
            {
                uplink.Unlocked = true;
                _ui.OpenUi(ent.Owner, StoreUiKey.Key, args.Actor);
                ResetCombination(ent.Comp, uplink);
            }
        }
        else
        {
            ResetCombination(ent.Comp, uplink);
        }
    }


    /// <remarks>Falls back to PDA if no preference is set.</remarks>
    public override UplinkPreference GetUplinkPreference(EntityUid mindEnt)
    {
        var mind = Comp<MindComponent>(mindEnt);

        // Default to PDA
        if (mind.UserId == null)
            return UplinkPreference.Pda;

        var prefs = _prefs.GetPreferences(mind.UserId.Value);
        if (prefs.SelectedCharacter is not HumanoidCharacterProfile profile
            || !profile.Loadouts.TryGetValue(AntagTraitorLoadout, out var roleLoadout)
            || !roleLoadout.SelectedLoadouts.TryGetValue(TraitorUplinkGroup, out var selectedLoadouts))
            return UplinkPreference.Pda;

        foreach (var loadout in selectedLoadouts)
        {
            if (loadout.Prototype == TraitorUplinkPDA)
                return UplinkPreference.Pda;
            if (loadout.Prototype == TraitorUplinkPen)
                return UplinkPreference.Pen;
            if (loadout.Prototype == TraitorUplinkImplant)
                return UplinkPreference.Implant;
        }

        return UplinkPreference.Pda;
    }

    private void OnReset(Entity<PenSpinComponent> ent, ref PenSpinResetMessage args)
    {
        if (TryComp<PenSpinUplinkComponent>(ent, out var uplink))
            ResetCombination(ent.Comp, uplink);
    }

    private void ResetCombination(PenSpinComponent spin, PenSpinUplinkComponent uplink)
    {
        uplink.CurrentCombination = new int[spin.CombinationLength];
        uplink.CurrentIndex = 0;
    }

    public override void SetupPenUplink(EntityUid pen)
    {
        EnsureComp<PenSpinUplinkComponent>(pen);
    }

    public override int[]? GetPenUplinkCode(EntityUid pen)
    {
        return TryComp<PenSpinUplinkComponent>(pen, out var uplink) ? uplink.Code : null;
    }

    private static bool IsValidDegree(PenSpinComponent comp, int degree)
    {
        return degree >= comp.MinDegree && degree <= comp.MaxDegree;
    }
}
