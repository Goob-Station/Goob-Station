// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Cyberdeck.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CyberdeckUserComponent : Component
{
    /// <summary>
    /// Entity that provides cyberdeck abilities to this entity.
    /// Used mostly for counting charges, and if null charges will just be infinite.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? ProviderEntity;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? ProjectionEntity;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? HackAction;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? VisionAction;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? ReturnAction;

    /// <summary>
    /// Used for Cyber vision ability checks, should be true if a player controls a projection.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool InProjection;

    [DataField]
    public string AlertId = "CyberdeckCharges";

    [DataField]
    public string DiveMusicId = "CyberdeckDiveLoop";

    [DataField]
    public int CyberVisionAbilityCost = 6;

    [DataField]
    public SoundSpecifier DiveStartSound =
        new SoundPathSpecifier("/Audio/_Goobstation/Effects/Cyberdeck/dive_intro.ogg", AudioParams.Default.WithVolume(6f));

    [DataField]
    public SoundSpecifier DiveExitSound =
        new SoundPathSpecifier("/Audio/_Goobstation/Effects/Cyberdeck/dive_exit.ogg", AudioParams.Default.WithVolume(6f));

    /// <summary>
    /// Sound that is played for the user when they hack something.
    /// </summary>
    [DataField]
    public SoundSpecifier UserHackingSound =
        new SoundPathSpecifier("/Audio/_Goobstation/Effects/Cyberdeck/hack_user.ogg", AudioParams.Default.WithVolume(6f));

    [DataField]
    public EntProtoId ProjectionEntityId = "CyberdeckProjection";

    [DataField]
    public EntProtoId HackActionId = "ActionCyberdeckHack";

    [DataField]
    public EntProtoId VisionActionId = "ActionCyberdeckVision";

    [DataField]
    public EntProtoId ReturnActionId = "ActionCyberdeckVisionReturn";
}
