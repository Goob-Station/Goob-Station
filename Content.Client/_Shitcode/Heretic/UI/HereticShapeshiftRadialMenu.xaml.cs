// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Polymorph;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Utility;

namespace Content.Client._Shitcode.Heretic.UI;

public sealed class HereticShapeshiftRadialMenu : RadialMenu
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private readonly SpriteSystem _spriteSystem;

    public event Action<ProtoId<PolymorphPrototype>>? SendHereticShapeshiftMessageAction;

    public EntityUid Entity { get; set; }

    public HereticShapeshiftRadialMenu()
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

        if (!_entityManager.TryGetComponent<ShapeshiftActionComponent>(Entity, out var action))
            return;

        foreach (var polymorph in action.Polymorphs)
        {
            if (!_prototypeManager.TryIndex(polymorph, out var polymorphPrototype))
                continue;

            var config = polymorphPrototype.Configuration;

            if (config.Entity == null)
                continue;

            var ent = _prototypeManager.Index(config.Entity.Value);

            var button = new HereticPolymorphMenuButton
            {
                SetSize = new Vector2(64, 64),
                ToolTip = ent.Name,
                ProtoId = polymorph
            };

            var texture = new TextureRect
            {
                VerticalAlignment = VAlignment.Center,
                HorizontalAlignment = HAlignment.Center,
                Texture = _spriteSystem.Frame0(ent),
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
                SendHereticShapeshiftMessageAction?.Invoke(castChild.ProtoId);
                Close();
            };
        }
    }

    public sealed class HereticPolymorphMenuButton : RadialMenuTextureButtonWithSector
    {
        public ProtoId<PolymorphPrototype> ProtoId { get; set; }
    }
}
