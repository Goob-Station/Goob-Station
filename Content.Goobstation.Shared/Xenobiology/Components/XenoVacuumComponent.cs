// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Xenobiology.Components;

/// <summary>
/// This handles the nozzle for xeno vacuums.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenoVacuumComponent : Component
{
    /// <summary>
    /// The EntityUid of the tank attached to this nozzle.
    /// </summary>
    public EntityUid? LinkedStorageTank;

    /// <summary>
    /// The sound played when the vacuum is used.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Effects/zzzt.ogg");

    /// <summary>
    /// The sound played when the tank is cleared.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? ClearSound = new SoundPathSpecifier("/Audio/Effects/trashbag3.ogg");
}
