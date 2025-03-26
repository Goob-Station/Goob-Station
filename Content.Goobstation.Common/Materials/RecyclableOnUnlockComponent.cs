using Robust.Shared.GameObjects;

namespace Content.Goobstation.Common.Materials;

/// <summary>
///     Component used to prevent recycling of locked lockers.
/// </summary>
[RegisterComponent]
public sealed partial class RecyclableOnUnlockComponent : Component;
