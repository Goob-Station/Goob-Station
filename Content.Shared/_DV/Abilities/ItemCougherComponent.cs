// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Abilities;

/// <summary>
/// Adds an action to cough up an item.
/// Other systems can enable this action when their conditions are met.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ItemCougherSystem))]
public sealed partial class ItemCougherComponent : Component
{
    /// <summary>
    /// The item to spawn after the coughing sound plays.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Item;

    /// <summary>
    /// The action to give the player.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Action;

    [DataField]
    public EntityUid? ActionEntity;

    /// <summary>
    /// Popup to show to everyone when coughing up an item.
    /// Gets "name" passed as the identity of the mob.
    /// </summary>
    [DataField(required: true)]
    public LocId CoughPopup;

    /// <summary>
    /// Sound played
    /// The sound length controls how long it takes for the item to spawn.
    /// </summary>
    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Animals/cat_hiss.ogg")
    {
        Params = new AudioParams
        {
            Variation = 0.15f
        }
    };
}