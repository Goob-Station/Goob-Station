// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.MalfAI;

/// <summary>
/// Marks an entity as a factory machine or entity that automatically connects to any Malfunctioning Silo.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfFactoryEntityComponent : Component
{
}
