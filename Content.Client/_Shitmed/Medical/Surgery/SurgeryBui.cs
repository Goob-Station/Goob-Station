using Content.Client._Shitmed.Xenonids.UI;
using Content.Client.Administration.UI.CustomControls;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._Shitmed.Medical.Surgery;

[UsedImplicitly]
public sealed class SurgeryBui : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private readonly SurgerySystem _system;
    [ViewVariables]
    private SurgeryWindow? _window;
    private EntityUid? _part;
    private bool _isBody;
    private (EntityUid Ent, EntProtoId Proto)? _surgery;
    private readonly List<EntProtoId> _previousSurgeries = new();
    public SurgeryBui(EntityUid owner, Enum uiKey) : base(owner, uiKey) => _system = _entities.System<SurgerySystem>();

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        if (_window is null
            || message is not SurgeryBuiRefreshMessage)
            return;

        RefreshUI();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not SurgeryBuiState s)
            return;

        Update(s);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _window?.Dispose();
    }

    private void Update(SurgeryBuiState state)
    {
        if (!_entities.TryGetComponent(_player.LocalEntity, out SurgeryTargetComponent? surgeryTargetComp)
            || !surgeryTargetComp.CanOperate)
            return;

        if (_window == null)
        {
            _window = new SurgeryWindow();
            _window.OnClose += Close;
            _window.Title = Loc.GetString("surgery-ui-window-title");

            _window.PartsButton.OnPressed += _ =>
            {
                _part = null;
                _isBody = false;
                _surgery = null;
                _previousSurgeries.Clear();
                View(ViewType.Parts);
            };

            _window.SurgeriesButton.OnPressed += _ =>
            {
                _surgery = null;
                _previousSurgeries.Clear();

                if (!_entities.TryGetNetEntity(_part, out var netPart)
                    || State is not SurgeryBuiState s
                    || !s.Choices.TryGetValue(netPart.Value, out var surgeries))
                    return;

                OnPartPressed(netPart.Value, surgeries);
            };

            _window.StepsButton.OnPressed += _ =>
            {
                if (!_entities.TryGetNetEntity(_part, out var netPart)
                    || _previousSurgeries.Count == 0)
                    return;

                var last = _previousSurgeries[^1];
                _previousSurgeries.RemoveAt(_previousSurgeries.Count - 1);

                if (_system.GetSingleton(last) is not { } previousId
                    || !_entities.TryGetComponent(previousId, out SurgeryComponent? previous))
                    return;

                OnSurgeryPressed((previousId, previous), netPart.Value, last);
            };
        }

        _window.Surgeries.DisposeAllChildren();
        _window.Steps.DisposeAllChildren();
        _window.Parts.DisposeAllChildren();
        View(ViewType.Parts);

        var oldSurgery = _surgery;
        var oldPart = _part;
        _part = null;
        _surgery = null;

        var options = new List<(NetEntity netEntity, EntityUid entity, string Name, BodyPartType? PartType)>();
        foreach (var choice in state.Choices.Keys)
            if (_entities.TryGetEntity(choice, out var ent))
            {
                if (_entities.TryGetComponent(ent, out BodyPartComponent? part))
                    options.Add((choice, ent.Value, _entities.GetComponent<MetaDataComponent>(ent.Value).EntityName, part.PartType));
                else if (_entities.TryGetComponent(ent, out BodyComponent? body))
                    options.Add((choice, ent.Value, _entities.GetComponent<MetaDataComponent>(ent.Value).EntityName, null));
            }

        options.Sort((a, b) =>
        {
            int GetScore(BodyPartType? partType)
            {
                return partType switch
                {
                    BodyPartType.Head => 1,
                    BodyPartType.Torso => 2,
                    BodyPartType.Arm => 3,
                    BodyPartType.Hand => 4,
                    BodyPartType.Leg => 5,
                    BodyPartType.Foot => 6,
                    // BodyPartType.Tail => 7, No tails yet!
                    BodyPartType.Other => 8,
                    _ => 9
                };
            }

            return GetScore(a.PartType) - GetScore(b.PartType);
        });

        foreach (var (netEntity, entity, partName, _) in options)
        {
            //var netPart = _entities.GetNetEntity(part.Owner);
            var surgeries = state.Choices[netEntity];
            var partButton = new XenoChoiceControl();

            partButton.Set(partName, null);
            partButton.Button.OnPressed += _ => OnPartPressed(netEntity, surgeries);

            _window.Parts.AddChild(partButton);

            foreach (var surgeryId in surgeries)
            {
                if (_system.GetSingleton(surgeryId) is not { } surgery ||
                    !_entities.TryGetComponent(surgery, out SurgeryComponent? surgeryComp))
                    continue;

                if (oldPart == entity && oldSurgery?.Proto == surgeryId)
                    OnSurgeryPressed((surgery, surgeryComp), netEntity, surgeryId);
            }

            if (oldPart == entity && oldSurgery == null)
                OnPartPressed(netEntity, surgeries);
        }


        if (!_window.IsOpen)
            _window.OpenCentered();
    }

    private void AddStep(EntProtoId stepId, NetEntity netPart, EntProtoId surgeryId)
    {
        if (_window == null
            || _system.GetSingleton(stepId) is not { } step)
            return;

        var stepName = new FormattedMessage();
        stepName.AddText(_entities.GetComponent<MetaDataComponent>(step).EntityName);
        var stepButton = new SurgeryStepButton { Step = step };
        stepButton.Button.OnPressed += _ => SendMessage(new SurgeryStepChosenBuiMsg(netPart, surgeryId, stepId, _isBody));

        _window.Steps.AddChild(stepButton);
    }

    private void OnSurgeryPressed(Entity<SurgeryComponent> surgery, NetEntity netPart, EntProtoId surgeryId)
    {
        if (_window == null)
            return;

        _part = _entities.GetEntity(netPart);
        _isBody = _entities.HasComponent<BodyComponent>(_part);
        _surgery = (surgery, surgeryId);

        _window.Steps.DisposeAllChildren();

        // This apparently does not consider if theres multiple surgery requirements in one surgery. Maybe thats fine.
        if (surgery.Comp.Requirement is { } requirementId && _system.GetSingleton(requirementId) is { } requirement)
        {
            var label = new XenoChoiceControl();
            label.Button.OnPressed += _ =>
            {
                _previousSurgeries.Add(surgeryId);

                if (_entities.TryGetComponent(requirement, out SurgeryComponent? requirementComp))
                    OnSurgeryPressed((requirement, requirementComp), netPart, requirementId);
            };

            var msg = new FormattedMessage();
            var surgeryName = _entities.GetComponent<MetaDataComponent>(requirement).EntityName;
            msg.AddMarkup($"[bold]{Loc.GetString("surgery-ui-window-require")}: {surgeryName}[/bold]");
            label.Set(msg, null);

            _window.Steps.AddChild(label);
            _window.Steps.AddChild(new HSeparator { Margin = new Thickness(0, 0, 0, 1) });
        }
        foreach (var stepId in surgery.Comp.Steps)
            AddStep(stepId, netPart, surgeryId);

        View(ViewType.Steps);
        RefreshUI();
    }

    private void OnPartPressed(NetEntity netPart, List<EntProtoId> surgeryIds)
    {
        if (_window == null)
            return;

        _part = _entities.GetEntity(netPart);
        _isBody = _entities.HasComponent<BodyComponent>(_part);
        _window.Surgeries.DisposeAllChildren();

        var surgeries = new List<(Entity<SurgeryComponent> Ent, EntProtoId Id, string Name)>();
        foreach (var surgeryId in surgeryIds)
        {
            if (_system.GetSingleton(surgeryId) is not { } surgery ||
                !_entities.TryGetComponent(surgery, out SurgeryComponent? surgeryComp))
            {
                continue;
            }

            var name = _entities.GetComponent<MetaDataComponent>(surgery).EntityName;
            surgeries.Add(((surgery, surgeryComp), surgeryId, name));
        }

        surgeries.Sort((a, b) =>
        {
            var priority = a.Ent.Comp.Priority.CompareTo(b.Ent.Comp.Priority);
            if (priority != 0)
                return priority;

            return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        });

        foreach (var surgery in surgeries)
        {
            var surgeryButton = new XenoChoiceControl();
            surgeryButton.Set(surgery.Name, null);

            surgeryButton.Button.OnPressed += _ => OnSurgeryPressed(surgery.Ent, netPart, surgery.Id);
            _window.Surgeries.AddChild(surgeryButton);
        }

        RefreshUI();
        View(ViewType.Surgeries);
    }

    private void RefreshUI()
    {
        if (_window == null
            || !_window.IsOpen
            || _part == null
            || !_entities.HasComponent<SurgeryComponent>(_surgery?.Ent)
            || !_entities.TryGetComponent(_player.LocalEntity ?? EntityUid.Invalid, out SurgeryTargetComponent? surgeryComp)
            || !surgeryComp.CanOperate)
            return;

        var next = _system.GetNextStep(Owner, _part.Value, _surgery.Value.Ent);
        var i = 0;
        foreach (var child in _window.Steps.Children)
        {
            if (child is not SurgeryStepButton stepButton)
                continue;

            var status = StepStatus.Incomplete;
            if (next == null)
                status = StepStatus.Complete;
            else if (next.Value.Surgery.Owner != _surgery.Value.Ent)
                status = StepStatus.Incomplete;
            else if (next.Value.Step == i)
                status = StepStatus.Next;
            else if (i < next.Value.Step)
                status = StepStatus.Complete;

            stepButton.Button.Disabled = status != StepStatus.Next;

            var stepName = new FormattedMessage();
            stepName.AddText(_entities.GetComponent<MetaDataComponent>(stepButton.Step).EntityName);

            if (status == StepStatus.Complete)
                stepButton.Button.Modulate = Color.Green;
            else
            {
                stepButton.Button.Modulate = Color.White;
                if (_player.LocalEntity is { } player
                    && status == StepStatus.Next
                    && !_system.CanPerformStep(player, Owner, _part.Value, stepButton.Step, false, out var popup, out var reason, out _))
                    stepButton.ToolTip = popup;
            }

            var texture = _entities.GetComponentOrNull<SpriteComponent>(stepButton.Step)?.Icon?.Default;
            stepButton.Set(stepName, texture);
            i++;
        }
    }

    private void View(ViewType type)
    {
        if (_window == null)
            return;

        _window.PartsButton.Parent!.Margin = new Thickness(0, 0, 0, 10);

        _window.Parts.Visible = type == ViewType.Parts;
        _window.PartsButton.Disabled = type == ViewType.Parts;

        _window.Surgeries.Visible = type == ViewType.Surgeries;
        _window.SurgeriesButton.Disabled = type != ViewType.Steps;

        _window.Steps.Visible = type == ViewType.Steps;
        _window.StepsButton.Disabled = type != ViewType.Steps || _previousSurgeries.Count == 0;

        if (_entities.TryGetComponent(_part, out MetaDataComponent? partMeta) &&
            _entities.TryGetComponent(_surgery?.Ent, out MetaDataComponent? surgeryMeta))
            _window.Title = $"Surgery - {partMeta.EntityName}, {surgeryMeta.EntityName}";
        else if (partMeta != null)
            _window.Title = $"Surgery - {partMeta.EntityName}";
        else
            _window.Title = "Surgery";
    }

    private enum ViewType
    {
        Parts,
        Surgeries,
        Steps
    }

    private enum StepStatus
    {
        Next,
        Complete,
        Incomplete
    }
}
