// SPDX-FileCopyrightText: 2024 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Toilet.Components
{
    /// <summary>
    /// Toilets that can be flushed, seats toggled up and down, items hidden in cistern.
    /// </summary>
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class ToiletComponent : Component
    {
        /// <summary>
        /// Toggles seat state.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool ToggleSeat;


        /// <summary>
        /// Sound to play when toggling toilet seat.
        /// </summary>
        [DataField]
        public SoundSpecifier SeatSound = new SoundPathSpecifier("/Audio/Effects/toilet_seat_down.ogg");
    }

    [Serializable, NetSerializable]
    public enum ToiletVisuals : byte
    {
        SeatVisualState,
    }

    [Serializable, NetSerializable]
    public enum SeatVisualState : byte
    {
        SeatUp,
        SeatDown
    }
}
