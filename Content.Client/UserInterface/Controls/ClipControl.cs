// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.UserInterface.Controls;

/// <summary>
/// Pretends to child controls that there's infinite space.
/// This can be used to make something like a <see cref="RichTextLabel"/> clip instead of wrapping.
/// </summary>
public sealed class ClipControl : Control
{
    private bool _clipHorizontal = true;
    private bool _clipVertical = true;

    public bool ClipHorizontal
    {
        get => _clipHorizontal;
        set
        {
            _clipHorizontal = value;
            InvalidateMeasure();
        }
    }

    public bool ClipVertical
    {
        get => _clipVertical;
        set
        {
            _clipVertical = value;
            InvalidateMeasure();
        }
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        if (ClipHorizontal)
            availableSize = availableSize with { X = float.PositiveInfinity };
        if (ClipVertical)
            availableSize = availableSize with { Y = float.PositiveInfinity };

        return base.MeasureOverride(availableSize);
    }

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        foreach (var child in Children)
        {
            child.Arrange(UIBox2.FromDimensions(Vector2.Zero, child.DesiredSize));
        }

        return finalSize;
    }
}