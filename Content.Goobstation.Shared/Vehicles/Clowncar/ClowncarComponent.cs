// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

//using Content.Shared.Actions.ActionTypes;

using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Vehicles.Clowncar;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedClowncarSystem))]
public sealed partial class ClowncarComponent : Component
{
    [DataField]
    [ViewVariables] //EntProtoId
    public string Container = "clowncar_container";

    [DataField]
    [ViewVariables]
    public EntProtoId ThankRiderAction = "ActionThankDriver";

    [DataField]
    [ViewVariables]
    public EntProtoId QuietInTheBackAction = "ActionQuietBackThere";

    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public int ThankCounter;

    #region Sound
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public SoundSpecifier CannonActivateSound = new SoundPathSpecifier("/Audio/_Goobstation/Vehicle/Clowncar/clowncar_activate_cannon.ogg");

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public SoundSpecifier CannonDeactivateSound = new SoundPathSpecifier("/Audio/_Goobstation/Vehicle/Clowncar/clowncar_deactivate_cannon.ogg");

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public SoundSpecifier FartSound = new SoundPathSpecifier("/Audio/_Goobstation/Vehicle/Clowncar/clowncar_fart.ogg");

    #endregion

}

//public sealed partial class ThankRiderAction : InstantActionEvent { }
public sealed partial class CannonAction : InstantActionEvent { }