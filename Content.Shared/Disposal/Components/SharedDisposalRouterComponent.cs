// SPDX-FileCopyrightText: 2020 Julian Giebel <j.giebel@netrocks.info>
// SPDX-FileCopyrightText: 2020 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Text.RegularExpressions;
using Robust.Shared.Serialization;

namespace Content.Shared.Disposal.Components
{
    public sealed partial class SharedDisposalRouterComponent : Component
    {
        public static readonly Regex TagRegex = new("^[a-zA-Z0-9, ]*$", RegexOptions.Compiled);

        [Serializable, NetSerializable]
        public sealed class DisposalRouterUserInterfaceState : BoundUserInterfaceState
        {
            public readonly string Tags;

            public DisposalRouterUserInterfaceState(string tags)
            {
                Tags = tags;
            }
        }

        [Serializable, NetSerializable]
        public sealed class UiActionMessage : BoundUserInterfaceMessage
        {
            public readonly UiAction Action;
            public readonly string Tags = "";

            public UiActionMessage(UiAction action, string tags)
            {
                Action = action;

                if (Action == UiAction.Ok)
                {
                    Tags = tags.Substring(0, Math.Min(tags.Length, 150));
                }
            }
        }

        [Serializable, NetSerializable]
        public enum UiAction
        {
            Ok
        }

        [Serializable, NetSerializable]
        public enum DisposalRouterUiKey
        {
            Key
        }
    }
}