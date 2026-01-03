using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared._Shitcode.Heretic.Components;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._Shitcode.Heretic.UI;

public sealed class EldritchIdRadialMenu : RadialMenu
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;

    private readonly SpriteSystem _spriteSystem;

    public event Action<EldritchIdConfiguration>? SendEldritchIdMessageAction;

    public EntityUid Entity { get; set; }

    public EldritchIdRadialMenu()
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);
        _spriteSystem = _entitySystem.GetEntitySystem<SpriteSystem>();
    }

    public void SetEntity(EntityUid uid)
    {
        Entity = uid;
        RefreshUI();
    }

    private void RefreshUI()
    {
        var main = FindControl<RadialContainer>("Main");
        if (main == null)
            return;

        if (!_entityManager.TryGetComponent<EldritchIdCardComponent>(Entity, out var eldritchId))
            return;

        foreach (var config in eldritchId.Configs)
        {
            var button = new HereticPolymorphMenuButton(config)
            {
                SetSize = new Vector2(64, 64),
                ToolTip = $"{config.FullName} ({config.JobTitle})",
            };

            var texture = new TextureRect
            {
                VerticalAlignment = VAlignment.Center,
                HorizontalAlignment = HAlignment.Center,
                Texture = _spriteSystem.GetPrototypeIcon(config.CardPrototype).Default,
                TextureScale = new Vector2(2f, 2f)
            };

            button.AddChild(texture);
            main.AddChild(button);
        }

        AddHereticPolymorphMenuButtonOnClickAction(main);
    }

    private void AddHereticPolymorphMenuButtonOnClickAction(RadialContainer mainControl)
    {
        if (mainControl == null)
            return;

        foreach (var child in mainControl.Children)
        {
            var castChild = child as HereticPolymorphMenuButton;

            if (castChild == null)
                continue;

            castChild.OnButtonUp += _ =>
            {
                SendEldritchIdMessageAction?.Invoke(castChild.Config);
                Close();
            };
        }
    }

    public sealed class HereticPolymorphMenuButton(EldritchIdConfiguration config) : RadialMenuTextureButtonWithSector
    {
        public EldritchIdConfiguration Config { get; set; } = config;
    }
}
