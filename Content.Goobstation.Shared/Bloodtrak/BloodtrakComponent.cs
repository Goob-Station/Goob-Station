// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Bloodtrak;

/// <summary>
/// Allows an item to track another entity based on DNA from a solution.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class BloodtrakComponent : Component
{
    /// <summary>
    /// The duration the tracker will remain on, before shutting off automatically.
    /// </summary>
    [DataField]
    public TimeSpan TrackingDuration = TimeSpan.FromSeconds(30f);

    /// <summary>
    /// The distance defined as being a medium distance away.
    /// </summary>
    [DataField]
    public float MediumDistance = 16f;

    /// <summary>
    /// The distance defined as being a short distance away.
    /// </summary>
    [DataField]
    public float CloseDistance = 8f;

    /// <summary>
    /// The distance defined as being close.
    /// </summary>
    [DataField]
    public float ReachedDistance = 1f;

    /// <summary>
    ///     Pinpointer arrow precision in radians.
    /// </summary>
    /// <remarks>
    /// 0.09 radians ≈ 5.16 degrees
    /// </remarks>
    [ViewVariables]
    public double Precision = 0.09;

    /// <summary>
    /// The current target of the tracker.
    /// </summary>
    [ViewVariables]
    public EntityUid? Target = null;

    /// <summary>
    /// Whether the tracker is currently active.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool IsActive = false;

    /// <summary>
    /// The current angle of the trackers arrow.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public Angle ArrowAngle;

    /// <summary>
    /// The current distance to the target.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public Distance DistanceToTarget = Distance.Unknown;

    /// <summary>
    /// How long until the next execution.
    /// </summary>
    [ViewVariables]
    public TimeSpan CooldownDuration = TimeSpan.FromSeconds(30f);

    /// <summary>
    /// When active tracking ends
    /// </summary>
    [ViewVariables]
    public TimeSpan ExpirationTime;

    /// <summary>
    /// When cooldown ends
    /// </summary>
    [ViewVariables]
    public TimeSpan CooldownEndTime = TimeSpan.Zero;

    [ViewVariables]
    public bool HasTarget => DistanceToTarget != Distance.Unknown;

}

[Serializable, NetSerializable]
public enum Distance : byte
{
    Unknown,
    Reached,
    Close,
    Medium,
    Far
}
