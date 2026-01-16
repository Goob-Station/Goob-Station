// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.UserInterface.Controls;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;
using Robust.Client.UserInterface;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Heretic.UI;

public sealed class HereticRitualRuneBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly EntityManager _ent = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    public HereticRitualRuneBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        var menu = this.CreateWindow<SimpleRadialMenu>();
        menu.Track(Owner);
        menu.SetButtons(CreateModels(_player.LocalEntity!.Value));

        menu.OpenOverMouseScreenPosition();
    }

    private IEnumerable<RadialMenuOption> CreateModels(EntityUid uid)
    {
        if (!_ent.TryGetComponent<HereticComponent>(uid, out var heretic))
            yield break;

        foreach (var rit in heretic.KnownRituals)
        {
            if (!_prot.TryIndex(rit, out var prototype))
                continue;

            yield return new RadialMenuActionOption<HereticRitualPrototype>(HandleMenuOptionClick, prototype)
            {
                Sprite = prototype.Icon,
                ToolTip = Loc.GetString(prototype.LocName),
            };
        }
    }

    private void HandleMenuOptionClick(HereticRitualPrototype protoId)
    {
        SendMessage(new HereticRitualMessage(protoId));
    }
}
