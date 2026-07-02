using Content.Server._Pirate.Plumbing.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.PowerCell;
using Content.Shared._Pirate.Plumbing;
using Content.Shared._Pirate.Plumbing.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.PowerCell;
using Content.Shared.UserInterface;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using SharedAppearanceSystem = Robust.Shared.GameObjects.SharedAppearanceSystem;

namespace Content.Server._Pirate.Plumbing.EntitySystems;

/// <summary>
///     Handles plumbing synthesizer machine behavior: basically a small reagent dispenser,
///     you can select a generatable reagent and it will produce it into its buffer container.
///     Uses power from an internal battery and the reagents have the same power cost as reagent dispensers.
///     Battery charging from APC is handled by <see cref="Content.Server.Power.EntitySystems.ChargerSystem"/>.
/// </summary>
[UsedImplicitly]
public sealed partial class PlumbingSynthesizerSystem : EntitySystem
{
    [Dependency] private SharedSolutionContainerSystem _solutionSystem = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;
    [Dependency] private PowerCellSystem _powerCell = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private PowerReceiverSystem _power = default!;
    [Dependency] private BatterySystem _battery = default!;

    private readonly Dictionary<EntityUid, TimeSpan> _nextUiUpdate = new();

    /// <summary>
    ///     How often to update the UI when open (in seconds).
    /// </summary>
    private const float UiUpdateIntervalSeconds = 0.5f;
    private static readonly TimeSpan _uiUpdateInterval = TimeSpan.FromSeconds(UiUpdateIntervalSeconds);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlumbingSynthesizerComponent, PlumbingDeviceUpdateEvent>(OnSynthesizerUpdate);
        SubscribeLocalEvent<PlumbingSynthesizerComponent, PlumbingSynthesizerToggleMessage>(OnToggle);
        SubscribeLocalEvent<PlumbingSynthesizerComponent, PlumbingSynthesizerSelectReagentMessage>(OnSelectReagent);
        SubscribeLocalEvent<PlumbingSynthesizerComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<PlumbingSynthesizerComponent, PlumbingPullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<PlumbingSynthesizerComponent, ComponentRemove>(OnComponentRemove);
    }

    /// <summary>
    ///     Update UI periodically when open.
    /// </summary>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<PlumbingSynthesizerComponent, ActivatableUIComponent>();
        while (query.MoveNext(out var uid, out var synth, out var activatableUI))
        {
            var uiOpen = activatableUI.Key != null && _ui.IsUiOpen(uid, activatableUI.Key);
            if (!uiOpen)
                continue;

            if (!_nextUiUpdate.TryGetValue(uid, out var nextUpdate) || curTime >= nextUpdate)
            {
                _nextUiUpdate[uid] = curTime + _uiUpdateInterval;
                UpdateUI((uid, synth));
            }
        }
    }

    private void OnComponentRemove(Entity<PlumbingSynthesizerComponent> ent, ref ComponentRemove args)
        => _nextUiUpdate.Remove(ent.Owner);

    private void OnSynthesizerUpdate(Entity<PlumbingSynthesizerComponent> ent, ref PlumbingDeviceUpdateEvent args)
    {
        RechargeCell(ent, args.dt);

        if (_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionName, out _, out var bufferCheck))
            _appearance.SetData(ent.Owner, PlumbingVisuals.Running, bufferCheck.Volume > 0);

        if (!ent.Comp.Enabled)
            return;

        if (ent.Comp.SelectedReagent == null)
            return;

        if (!_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionName, out var bufferEnt, out var buffer))
            return;

        var availableSpace = buffer.AvailableVolume;
        if (availableSpace <= 0)
            return;

        if (!ent.Comp.GeneratableReagents.TryGetValue(ent.Comp.SelectedReagent.Value, out var powerDrain))
            return;

        var toGenerate = FixedPoint2.Min(availableSpace, buffer.MaxVolume);

        var powerNeeded = powerDrain * (float)toGenerate;
        if (!_powerCell.HasCharge(ent.Owner, powerNeeded))
        {
            if (!_powerCell.TryGetBatteryFromSlot(ent.Owner, out var batteryCheck))
                return;

            var availableCharge = batteryCheck.CurrentCharge;
            if (availableCharge <= 0)
                return;

            var affordableUnits = FixedPoint2.New((int)(availableCharge / powerDrain));
            if (affordableUnits <= 0)
                return;

            toGenerate = FixedPoint2.Min(toGenerate, affordableUnits);
            powerNeeded = powerDrain * (float)toGenerate;
        }

        if (!_powerCell.TryUseCharge(ent.Owner, powerNeeded))
            return;

        _solutionSystem.TryAddReagent(bufferEnt.Value, new ReagentId(ent.Comp.SelectedReagent.Value, null), toGenerate, out _);

        _appearance.SetData(ent.Owner, PlumbingVisuals.Running, buffer.Volume > 0);
        UpdateUI(ent);
    }

    /// <summary>
    ///     Tops up the inserted power cell from grid power. Done here because the stock Charger
    ///     only wakes on cell insert/remove, so a cell drained in-place never resumes charging.
    /// </summary>
    private void RechargeCell(Entity<PlumbingSynthesizerComponent> ent, float dt)
    {
        if (ent.Comp.CellRechargeRate <= 0f || !_power.IsPowered(ent.Owner))
            return;

        if (!_powerCell.TryGetBatteryFromSlot(ent.Owner, out var batteryUid, out var battery))
            return;

        if (battery.CurrentCharge >= battery.MaxCharge)
            return;

        _battery.SetCharge(batteryUid.Value, battery.CurrentCharge + ent.Comp.CellRechargeRate * dt, battery);
    }

    /// <summary>
    ///     Only allow pulling the selected reagent. Maybe not needed but trying to fix an issue
    /// </summary>
    private void OnPullAttempt(Entity<PlumbingSynthesizerComponent> ent, ref PlumbingPullAttemptEvent args)
    {
        if (ent.Comp.SelectedReagent == null || args.ReagentPrototype != ent.Comp.SelectedReagent)
        {
            args.Cancelled = true;
        }
    }

    private void OnToggle(Entity<PlumbingSynthesizerComponent> ent, ref PlumbingSynthesizerToggleMessage args)
    {
        ent.Comp.Enabled = args.Enabled;
        DirtyField(ent, ent.Comp, nameof(PlumbingSynthesizerComponent.Enabled));
        ClickSound(ent.Owner);
        UpdateUI(ent);
    }

    private void OnSelectReagent(Entity<PlumbingSynthesizerComponent> ent, ref PlumbingSynthesizerSelectReagentMessage args)
    {
        if (args.ReagentId == null)
        {
            ent.Comp.SelectedReagent = null;
        }
        else if (ent.Comp.GeneratableReagents.ContainsKey(args.ReagentId))
        {
            ent.Comp.SelectedReagent = args.ReagentId;
        }

        DirtyField(ent, ent.Comp, nameof(PlumbingSynthesizerComponent.SelectedReagent));
        ClickSound(ent.Owner);
        UpdateUI(ent);
    }

    private void OnUIOpened(Entity<PlumbingSynthesizerComponent> ent, ref BoundUIOpenedEvent args)
        => UpdateUI(ent);

    private void UpdateUI(Entity<PlumbingSynthesizerComponent> ent)
    {
        if (!_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.BufferSolutionName, out _, out var buffer))
            return;

        var batteryCharge = 0f;
        if (_powerCell.TryGetBatteryFromSlot(ent.Owner, out var battery))
        {
            batteryCharge = battery.MaxCharge > 0
                ? battery.CurrentCharge / battery.MaxCharge
                : 0f;
        }

        var generatableReagents = new Dictionary<string, float>();
        foreach (var (reagent, drain) in ent.Comp.GeneratableReagents)
        {
            generatableReagents[reagent.Id] = drain;
        }

        var bufferContents = new Dictionary<string, FixedPoint2>();
        foreach (var reagent in buffer.Contents)
        {
            bufferContents[reagent.Reagent.Prototype] = reagent.Quantity;
        }

        var state = new PlumbingSynthesizerBoundUserInterfaceState(
            generatableReagents,
            ent.Comp.SelectedReagent?.Id,
            bufferContents,
            ent.Comp.Enabled,
            batteryCharge);

        _ui.SetUiState(ent.Owner, PlumbingSynthesizerUiKey.Key, state);
    }

    private void ClickSound(EntityUid uid)
    {
        if (TryComp<PlumbingDeviceComponent>(uid, out var device))
            _audio.PlayPvs(device.ClickSound, uid, AudioParams.Default.WithVolume(-2f));
    }
}
