// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid.Markings;
using Content.Shared.MagicMirror;
using Robust.Client.UserInterface;

namespace Content.Client.MagicMirror;

public sealed class MagicMirrorBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private MagicMirrorWindow? _window;

    public MagicMirrorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MagicMirrorWindow>();

        _window.OnHairSelected += tuple => SelectHair(MagicMirrorCategory.Hair, tuple.id, tuple.slot);
        _window.OnHairColorChanged += args => ChangeColor(MagicMirrorCategory.Hair, args.marking, args.slot);
        _window.OnHairSlotAdded += delegate () { AddSlot(MagicMirrorCategory.Hair); };
        _window.OnHairSlotRemoved += args => RemoveSlot(MagicMirrorCategory.Hair, args);

        _window.OnFacialHairSelected += tuple => SelectHair(MagicMirrorCategory.FacialHair, tuple.id, tuple.slot);
        _window.OnFacialHairColorChanged +=
            args => ChangeColor(MagicMirrorCategory.FacialHair, args.marking, args.slot);
        _window.OnFacialHairSlotAdded += delegate () { AddSlot(MagicMirrorCategory.FacialHair); };
        _window.OnFacialHairSlotRemoved += args => RemoveSlot(MagicMirrorCategory.FacialHair, args);
    }

    private void SelectHair(MagicMirrorCategory category, string marking, int slot)
    {
        SendMessage(new MagicMirrorSelectMessage(category, marking, slot));
    }

    private void ChangeColor(MagicMirrorCategory category, Marking marking, int slot)
    {
        SendMessage(new MagicMirrorChangeColorMessage(category, new(marking.MarkingColors), slot));
    }

    private void RemoveSlot(MagicMirrorCategory category, int slot)
    {
        SendMessage(new MagicMirrorRemoveSlotMessage(category, slot));
    }

    private void AddSlot(MagicMirrorCategory category)
    {
        SendMessage(new MagicMirrorAddSlotMessage(category));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not MagicMirrorUiState data || _window == null)
        {
            return;
        }

        _window.UpdateState(data);
    }
}
