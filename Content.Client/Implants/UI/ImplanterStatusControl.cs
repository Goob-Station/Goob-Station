// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Message;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Shared.Implants.Components;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.Implants.UI;

public sealed class ImplanterStatusControl : Control
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    private readonly ImplanterComponent _parent;
    private readonly RichTextLabel _label;

    public ImplanterStatusControl(ImplanterComponent parent)
    {
        IoCManager.InjectDependencies(this);
        _parent = parent;
        _label = new RichTextLabel { StyleClasses = { StyleNano.StyleClassItemStatus } };
        _label.MaxWidth = 350;
        AddChild(new ClipControl { Children = { _label } });

        Update();
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
        if (!_parent.UiUpdateNeeded)
            return;

        Update();
    }

    private void Update()
    {
        _parent.UiUpdateNeeded = false;

        var modeStringLocalized = _parent.CurrentMode switch
        {
            ImplanterToggleMode.Draw => Loc.GetString("implanter-draw-text"),
            ImplanterToggleMode.Inject => Loc.GetString("implanter-inject-text"),
            _ => Loc.GetString("injector-invalid-injector-toggle-mode")
        };

        if (_parent.CurrentMode == ImplanterToggleMode.Draw)
        {
            string implantName = _parent.DeimplantChosen != null
                ? (_prototype.TryIndex(_parent.DeimplantChosen.Value, out EntityPrototype? implantProto) ? implantProto.Name : Loc.GetString("implanter-empty-text"))
                : Loc.GetString("implanter-empty-text");

            _label.SetMarkup(Loc.GetString("implanter-label-draw",
                    ("implantName", implantName),
                    ("modeString", modeStringLocalized)));
        }
        else
        {
            var implantName = _parent.ImplanterSlot.HasItem
                ? _parent.ImplantData.Item1
                : Loc.GetString("implanter-empty-text");

            _label.SetMarkup(Loc.GetString("implanter-label-inject",
                    ("implantName", implantName),
                    ("modeString", modeStringLocalized)));
        }
    }
}