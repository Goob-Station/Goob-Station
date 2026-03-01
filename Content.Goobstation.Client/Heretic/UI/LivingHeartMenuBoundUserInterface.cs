// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Lobby;
using Content.Client.UserInterface.Controls;
using Content.Shared.Heretic;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using System.Numerics;
using static Robust.Client.UserInterface.Control;

namespace Content.Goobstation.Client.Heretic.UI;

public sealed partial class LivingHeartMenuBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;
    private readonly LobbyUIController _controller;

    public LivingHeartMenuBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
        _controller = _ui.GetUIController<LobbyUIController>();
    }

    protected override void Open()
    {
        base.Open();

        var menu = this.CreateWindow<SimpleRadialMenu>();
        menu.Track(Owner);
        menu.SetButtons(CreateModels(Owner));

        menu.OpenOverMouseScreenPosition();
    }

    private IEnumerable<RadialMenuOption> CreateModels(EntityUid uid)
    {
        var player = _player.LocalEntity;

        if (!_ent.TryGetComponent<HereticComponent>(player, out var heretic))
            yield break;

        foreach (var target in heretic.SacrificeTargets)
        {
            if (!_ent.TryGetEntity(target.Entity, out var ent) || !_ent.EntityExists(ent))
                ent = _controller.LoadProfileEntity(target.Profile, _prot.Index(target.Job), true);

            var texture = new SpriteView(ent.Value, _ent)
            {
                OverrideDirection = Direction.South,
                VerticalAlignment = VAlignment.Center,
                SetSize = new Vector2(64, 64),
                VerticalExpand = true,
                Stretch = SpriteView.StretchMode.Fill,
            };

            // convert SpriteView texture into SpriteSpecifier for use with RadialMenuActionOption.Sprite.

            yield return new RadialMenuActionOption<NetEntity>(HandleMenuOptionClick, target.Entity)
            {
                SpriteView = texture,
                ToolTip = target.Profile.Name,
            };
        }
    }

    private void HandleMenuOptionClick(NetEntity netent)
    {
        SendMessage(new EventHereticLivingHeartActivate() { Target = netent });
    }
}
