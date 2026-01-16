// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.UserInterface.Controls;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Heretic.Prototypes;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Heretic.UI;

public sealed class CarvingKnifeBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly EntityManager _ent = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;

    public CarvingKnifeBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
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
        if (!_ent.TryGetComponent<CarvingKnifeComponent>(uid, out var knife))
            yield break;

        foreach (var ammo in knife.Carvings)
        {
            if (!_prot.TryIndex(ammo, out var prototype))
                continue;

            yield return new RadialMenuActionOption<RuneCarvingPrototype>(HandleMenuOptionClick, prototype)
            {
                Sprite = prototype.Icon,
                ToolTip = Loc.GetString(prototype.Desc),
            };
        }
    }

    private void HandleMenuOptionClick(RuneCarvingPrototype prototype)
    {
        SendMessage(new RuneCarvingSelectedMessage(prototype));
    }
}
