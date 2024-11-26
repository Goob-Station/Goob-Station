using System.Numerics;
using Content.Client.Message;
using Content.Client.Paper.UI;
using Content.Shared.CCVar;
using Content.Shared.Movement.Components;
using Content.Shared.Tips;
using Robust.Client.GameObjects;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Client.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using static Content.Client.Tips.MariahUI;

namespace Content.Client.Tips;

public sealed class MariahController : UIController
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IResourceCache _resCache = default!;
    [UISystemDependency] private readonly AudioSystem _audio = default!;

    public const float Padding = 50;
    public static Angle WaddleRotation = Angle.FromDegrees(10);

    private EntityUid _entity;
    private float _secondsUntilNextState;
    private int _previousStep = 0;
    private MariahEvent? _currentMessage;
    private readonly Queue<MariahEvent> _queuedMessages = new();

    public override void Initialize()
    {
        base.Initialize();
        UIManager.OnScreenChanged += OnScreenChanged;
        SubscribeNetworkEvent<MariahEvent>(OnMariahEvent);
    }

    private void OnMariahEvent(MariahEvent msg, EntitySessionEventArgs args)
    {
        _queuedMessages.Enqueue(msg);
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        var screen = UIManager.ActiveScreen;
        if (screen == null)
        {
            _queuedMessages.Clear();
            return;
        }

        var mariah = screen.GetOrAddWidget<MariahUI>();
        _secondsUntilNextState -= args.DeltaSeconds;

        if (_secondsUntilNextState <= 0)
            NextState(mariah);
        else
        {
            var pos = UpdatePosition(mariah, screen.Size, args); ;
            LayoutContainer.SetPosition(mariah, pos);
        }
    }

    private Vector2 UpdatePosition(MariahUI mariah, Vector2 screenSize, FrameEventArgs args)
    {
        if (_currentMessage == null)
            return default;

        var slideTime = _currentMessage.SlideTime;

        var offset = mariah.State switch
        {
            MariahState.Hidden => 0,
            MariahState.Revealing => Math.Clamp(1 - _secondsUntilNextState / slideTime, 0, 1),
            MariahState.Hiding => Math.Clamp(_secondsUntilNextState / slideTime, 0, 1),
            _ => 1,
        };

        var waddle = _currentMessage.WaddleInterval;

        if (_currentMessage == null
            || waddle <= 0
            || mariah.State == MariahState.Hidden
            || mariah.State == MariahState.Speaking
            || !EntityManager.TryGetComponent(_entity, out SpriteComponent? sprite))
        {
            return new Vector2(screenSize.X - offset * (mariah.DesiredSize.X + Padding), (screenSize.Y - mariah.DesiredSize.Y) / 2);
        }

        var numSteps = (int) Math.Ceiling(slideTime / waddle);
        var curStep = (int) Math.Floor(numSteps * offset);
        var stepSize = (mariah.DesiredSize.X + Padding) / numSteps;

        if (curStep != _previousStep)
        {
            _previousStep = curStep;
            sprite.Rotation = sprite.Rotation > 0
                ? -WaddleRotation
                : WaddleRotation;

            if (EntityManager.TryGetComponent(_entity, out FootstepModifierComponent? step))
            {
                var audioParams = step.FootstepSoundCollection.Params
                    .AddVolume(-7f)
                    .WithVariation(0.1f);
                _audio.PlayGlobal(step.FootstepSoundCollection, EntityUid.Invalid, audioParams);
            }
        }

        return new Vector2(screenSize.X - stepSize * curStep, (screenSize.Y - mariah.DesiredSize.Y) / 2);
    }

    private void NextState(MariahUI mariah)
    {
        SpriteComponent? sprite;
        switch (mariah.State)
        {
            case MariahState.Hidden:
                if (!_queuedMessages.TryDequeue(out var next))
                    return;

                if (next.Proto != null)
                {
                    _entity = EntityManager.SpawnEntity(next.Proto, MapCoordinates.Nullspace);
                    mariah.ModifyLayers = false;
                }
                else
                {
                    _entity = EntityManager.SpawnEntity(_cfg.GetCVar(CCVars.MariahEntity), MapCoordinates.Nullspace);
                    mariah.ModifyLayers = true;
                }
                if (!EntityManager.TryGetComponent(_entity, out sprite))
                    return;
                if (!EntityManager.HasComponent<PaperVisualsComponent>(_entity))
                {
                    var paper = EntityManager.AddComponent<PaperVisualsComponent>(_entity);
                    paper.BackgroundImagePath = "/Textures/Interface/Paper/paper_background_default.svg.96dpi.png";
                    paper.BackgroundPatchMargin = new(16f, 16f, 16f, 16f);
                    paper.BackgroundModulate = new(255, 255, 204);
                    paper.FontAccentColor = new(0, 0, 0);
                }
                mariah.InitLabel(EntityManager.GetComponentOrNull<PaperVisualsComponent>(_entity), _resCache);

                var scale = sprite.Scale;
                if (mariah.ModifyLayers)
                {
                    sprite.Scale = Vector2.One;
                }
                else
                {
                    sprite.Scale = new Vector2(3, 3);
                }
                mariah.Entity.SetEntity(_entity);
                mariah.Entity.Scale = scale;

                _currentMessage = next;
                _secondsUntilNextState = next.SlideTime;
                mariah.State = MariahState.Revealing;
                _previousStep = 0;
                if (mariah.ModifyLayers)
                {
                    sprite.LayerSetAnimationTime("revealing", 0);
                    sprite.LayerSetVisible("revealing", true);
                    sprite.LayerSetVisible("speaking", false);
                    sprite.LayerSetVisible("hiding", false);
                }
                sprite.Rotation = 0;
                mariah.Label.SetMarkupPermissive(_currentMessage.Msg);
                mariah.Label.Visible = false;
                mariah.LabelPanel.Visible = false;
                mariah.Visible = true;
                sprite.Visible = true;
                break;

            case MariahState.Revealing:
                mariah.State = MariahState.Speaking;
                if (!EntityManager.TryGetComponent(_entity, out sprite))
                    return;
                sprite.Rotation = 0;
                _previousStep = 0;
                if (mariah.ModifyLayers)
                {
                    sprite.LayerSetAnimationTime("speaking", 0);
                    sprite.LayerSetVisible("revealing", false);
                    sprite.LayerSetVisible("speaking", true);
                    sprite.LayerSetVisible("hiding", false);
                }
                mariah.Label.Visible = true;
                mariah.LabelPanel.Visible = true;
                mariah.InvalidateArrange();
                mariah.InvalidateMeasure();
                if (_currentMessage != null)
                    _secondsUntilNextState = _currentMessage.SpeakTime;

                break;

            case MariahState.Speaking:
                mariah.State = MariahState.Hiding;
                if (!EntityManager.TryGetComponent(_entity, out sprite))
                    return;
                if (mariah.ModifyLayers)
                {
                    sprite.LayerSetAnimationTime("hiding", 0);
                    sprite.LayerSetVisible("revealing", false);
                    sprite.LayerSetVisible("speaking", false);
                    sprite.LayerSetVisible("hiding", true);
                }
                mariah.LabelPanel.Visible = false;
                if (_currentMessage != null)
                    _secondsUntilNextState = _currentMessage.SlideTime;
                break;

            default: // finished hiding

                EntityManager.DeleteEntity(_entity);
                _entity = default;
                mariah.Visible = false;
                _currentMessage = null;
                _secondsUntilNextState = 0;
                mariah.State = MariahState.Hidden;
                break;
        }
    }

    private void OnScreenChanged((UIScreen? Old, UIScreen? New) ev)
    {
        ev.Old?.RemoveWidget<MariahUI>();
        _currentMessage = null;
        EntityManager.DeleteEntity(_entity);
    }
}
