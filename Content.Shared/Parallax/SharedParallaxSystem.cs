// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.Parallax;

/// <summary>
/// Handles per-map parallax in sim. Out of sim parallax is handled by ParallaxManager.
/// </summary>
public abstract class SharedParallaxSystem: EntitySystem
{
    [Serializable, NetSerializable]
    protected sealed class ParallaxComponentState : ComponentState
    {
        public string Parallax = string.Empty;
    }
}