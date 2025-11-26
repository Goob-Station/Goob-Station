// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Input;
using Robust.Shared.Utility;

namespace Content.Client.Guidebook.RichText;

[UsedImplicitly]
public sealed class TextLinkTag : IMarkupTagHandler
{
    public string Name => "textlink";

    /// <inheritdoc/>
    public bool TryCreateControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        if (!node.Value.TryGetString(out var text)
            || !node.Attributes.TryGetValue("link", out var linkParameter)
            || !linkParameter.TryGetString(out var link))
        {
            control = null;
            return false;
        }

        var label = new Label();
        label.Text = text;

        label.MouseFilter = Control.MouseFilterMode.Stop;
        label.FontColorOverride = Color.CornflowerBlue;
        label.DefaultCursorShape = Control.CursorShape.Hand;

        label.OnMouseEntered += _ => label.FontColorOverride = Color.LightSkyBlue;
        label.OnMouseExited += _ => label.FontColorOverride = Color.CornflowerBlue;
        label.OnKeyBindDown += args => OnKeybindDown(args, link, label);

        control = label;
        return true;
    }

    private void OnKeybindDown(GUIBoundKeyEventArgs args, string link, Control? control)
    {
        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        if (control == null)
            return;

        var current = control;
        while (current != null)
        {
            current = current.Parent;

            if (current is not ILinkClickHandler handler)
                continue;
            handler.HandleClick(link);
            return;
        }
    }
}

public interface ILinkClickHandler
{
    public void HandleClick(string link);
}
