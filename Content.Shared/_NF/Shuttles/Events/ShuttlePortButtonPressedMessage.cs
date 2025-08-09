// Monolith - This file is licensed under AGPLv3
// Copyright (c) 2025 Monolith
// See AGPLv3.txt for details.

using Robust.Shared.Serialization;

namespace Content.Shared._NF.Shuttles.Events
{
    /// <summary>
    /// Sent when a network port button is pressed on the shuttle console.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class ShuttlePortButtonPressedMessage : BoundUserInterfaceMessage
    {
        /// <summary>
        /// The source port identifier from the shuttle console.
        /// </summary>
        public string SourcePort { get; set; } = string.Empty;
    }
}
