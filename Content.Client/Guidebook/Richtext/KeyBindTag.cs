// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using Robust.Client.Input;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Utility;

namespace Content.Client.Guidebook.Richtext;

[UsedImplicitly]
public sealed class KeyBindTag : IMarkupTag
{
    [Dependency] private readonly IInputManager _inputManager = default!;

    public string Name => "keybind";

    public string TextBefore(MarkupNode node)
    {
        if (!node.Value.TryGetString(out var keyBindName))
            return "";

        if (!_inputManager.TryGetKeyBinding(keyBindName, out var binding))
            return keyBindName;

        return binding.GetKeyString();
    }
}