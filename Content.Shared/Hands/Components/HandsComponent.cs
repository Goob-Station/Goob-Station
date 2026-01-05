// SPDX-FileCopyrightText: 2017 PJB3005 <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2018 clusterfack <clusterfack@users.noreply.github.com>
// SPDX-FileCopyrightText: 2019 Silver <Silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2019 ZelteHonor <gabrieldionbouchard@gmail.com>
// SPDX-FileCopyrightText: 2020 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2020 ComicIronic <comicironic@gmail.com>
// SPDX-FileCopyrightText: 2020 Jackson Lewis <inquisitivepenguin@protonmail.com>
// SPDX-FileCopyrightText: 2020 Tyler Young <tyler.young@impromptu.ninja>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2020 zumorica <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Clyybber <darkmine956@gmail.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2021 Remie Richards <remierichards@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 collinlunn <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 Paul <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DisplacementMap;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Hands.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
[Access(typeof(SharedHandsSystem))]
public sealed partial class HandsComponent : Component
{
    /// <summary>
    ///     The currently active hand.
    /// </summary>
    [DataField]
    public string? ActiveHandId;

    /// <summary>
    /// Dictionary relating a unique hand ID corresponding to a container slot on the attached entity to a class containing information about the Hand itself.
    /// </summary>
    [DataField]
    public Dictionary<string, Hand> Hands = new();

    /// <summary>
    /// The number of hands
    /// </summary>
    [ViewVariables]
    public int Count => Hands.Count;

    /// <summary>
    ///     List of hand-names. These are keys for <see cref="Hands"/>. The order of this list determines the order in which hands are iterated over.
    /// </summary>
    [DataField]
    public List<string> SortedHands = new();

    /// <summary>
    ///     If true, the items in the hands won't be affected by explosions.
    /// </summary>
    [DataField]
    public bool DisableExplosionRecursion;

    /// <summary>
    ///     Modifies the speed at which items are thrown.
    /// </summary>
    [DataField]
    public float BaseThrowspeed = 15f; // Goob

    /// <summary>
    ///     Distance after which longer throw targets stop increasing throw impulse.
    /// </summary>
    [DataField]
    public float ThrowRange = 8f;

    /// <summary>
    ///     Whether or not to add in-hand sprites for held items. Some entities (e.g., drones) don't want these.
    ///     Used by the client.
    /// </summary>
    [DataField]
    public bool ShowInHands = true;

    /// <summary>
    ///     Data about the current sprite layers that the hand is contributing to the owner entity. Used for sprite in-hands.
    ///     Used by the client.
    /// </summary>
    public readonly Dictionary<HandLocation, HashSet<string>> RevealedLayers = new();

    /// <summary>
    ///     The time at which throws will be allowed again.
    /// </summary>
    [DataField, AutoPausedField]
    public TimeSpan NextThrowTime;

    /// <summary>
    ///     The minimum time inbetween throws.
    /// </summary>
    [DataField]
    public TimeSpan ThrowCooldown = TimeSpan.FromSeconds(0.5f);

    /// <summary>
    ///     Fallback displacement map applied to all sprites in the hand, unless otherwise specified
    /// </summary>
    [DataField]
    public DisplacementData? HandDisplacement;

    /// <summary>
    ///     If defined, applies to all sprites in the left hand, ignoring <see cref="HandDisplacement"/>
    /// </summary>
    [DataField]
    public DisplacementData? LeftHandDisplacement;

    /// <summary>
    ///     If defined, applies to all sprites in the right hand, ignoring <see cref="HandDisplacement"/>
    /// </summary>
    [DataField]
    public DisplacementData? RightHandDisplacement;

    /// <summary>
    /// If false, hands cannot be stripped, and they do not show up in the stripping menu.
    /// </summary>
    [DataField]
    public bool CanBeStripped = true;
}

[DataDefinition]
[Serializable, NetSerializable]
public partial record struct Hand
{
    [DataField]
    public HandLocation Location = HandLocation.Right;

    public Hand()
    {

    }

    public Hand(HandLocation location)
    {
        Location = location;
    }
}

[Serializable, NetSerializable]
public sealed class HandsComponentState : ComponentState
{
    public readonly Dictionary<string, Hand> Hands;
    public readonly List<string> SortedHands;
    public readonly string? ActiveHandId;

    public HandsComponentState(HandsComponent handComp)
    {
        // cloning lists because of test networking.
        Hands = new(handComp.Hands);
        SortedHands = new(handComp.SortedHands);
        ActiveHandId = handComp.ActiveHandId;
    }
}

/// <summary>
///     What side of the body this hand is on.
/// </summary>
/// <seealso cref="HandUILocation"/>
/// <seealso cref="HandLocationExt"/>
public enum HandLocation : byte
{
    Left,
    Middle,
    Right
}

/// <summary>
/// What side of the UI a hand is on.
/// </summary>
/// <seealso cref="HandLocationExt"/>
/// <seealso cref="HandLocation"/>
public enum HandUILocation : byte
{
    Left,
    Right
}

/// <summary>
/// Helper functions for working with <see cref="HandLocation"/>.
/// </summary>
public static class HandLocationExt
{
    /// <summary>
    /// Convert a <see cref="HandLocation"/> into the appropriate <see cref="HandUILocation"/>.
    /// This maps "middle" hands to <see cref="HandUILocation.Right"/>.
    /// </summary>
    public static HandUILocation GetUILocation(this HandLocation location)
    {
        return location switch
        {
            HandLocation.Left => HandUILocation.Left,
            HandLocation.Middle => HandUILocation.Right,
            HandLocation.Right => HandUILocation.Right,
            _ => throw new ArgumentOutOfRangeException(nameof(location), location, null)
        };
    }
}
