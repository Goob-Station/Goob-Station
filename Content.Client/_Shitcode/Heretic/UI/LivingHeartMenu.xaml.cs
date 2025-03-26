using System.Numerics;
using Content.Client.Lobby;
using Content.Client.UserInterface.Controls;
using Content.Shared.Heretic;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client._Shitcode.Heretic.UI;

public sealed class LivingHeartMenu : RadialMenu
{
    [Dependency] private readonly EntityManager _ent = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private readonly LobbyUIController _controller;

    public EntityUid Entity { get; private set; }

    public event Action<NetEntity>? SendActivateMessageAction;

    public LivingHeartMenu()
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);

        _controller = UserInterfaceManager.GetUIController<LobbyUIController>();
    }

    public void SetEntity(EntityUid ent)
    {
        Entity = ent;
        UpdateUI();
    }

    private void UpdateUI()
    {
        var main = FindControl<RadialContainer>("Main");
        if (main == null) return;

        var player = _player.LocalEntity;

        if (!_ent.TryGetComponent<HereticComponent>(player, out var heretic))
            return;

        foreach (var target in heretic.SacrificeTargets)
        {
            if (!_ent.TryGetEntity(target.Entity, out var ent) || !_ent.EntityExists(ent))
                ent = _controller.LoadProfileEntity(target.Profile, _prot.Index(target.Job), true);

            var button = new EmbeddedEntityMenuButton
            {
                SetSize = new Vector2(64, 64),
                ToolTip = target.Profile.Name,
                NetEntity = target.Entity,
            };

            var texture = new SpriteView(ent.Value, _ent)
            {
                OverrideDirection = Direction.South,
                VerticalAlignment = VAlignment.Center,
                SetSize = new Vector2(64, 64),
                VerticalExpand = true,
                Stretch = SpriteView.StretchMode.Fill,
            };
            button.AddChild(texture);

            main.AddChild(button);
        }
        AddAction(main);
    }

    private void AddAction(RadialContainer main)
    {
        if (main == null)
            return;

        foreach (var child in main.Children)
        {
            var castChild = child as EmbeddedEntityMenuButton;
            if (castChild == null)
                continue;

            castChild.OnButtonUp += _ =>
            {
                SendActivateMessageAction?.Invoke(castChild.NetEntity);
                Close();
            };
        }
    }

    public sealed class EmbeddedEntityMenuButton : RadialMenuTextureButtonWithSector
    {
        public NetEntity NetEntity;
    }
}
