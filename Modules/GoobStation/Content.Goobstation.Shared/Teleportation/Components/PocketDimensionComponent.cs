// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Teleportation.Components;

/// <summary>
/// Creates a map for a pocket dimension on spawn.
/// When activated by alt verb, spawns a portal to this dimension or closes it.
/// </summary>
[RegisterComponent]
[Access(typeof(PocketDimensionSystem))]
public sealed partial class PocketDimensionComponent : Component
{
    /// <summary>
    /// Whether this pocket dimension portal is enabled.
    /// </summary>
    [ViewVariables]
    public bool PortalEnabled = false;

    /// <summary>
    /// The portal in the pocket dimension. Created when the entry portal is first opened.
    /// </summary>
    [ViewVariables]
    public EntityUid? ExitPortal;

    /// <summary>
    /// The pocket dimension map. Created when the entry portal is first opened.
    /// </summary>
    [ViewVariables]
    public EntityUid? PocketDimensionMap;

    /// <summary>
    /// Path to the pocket dimension's map file
    /// </summary>
    [DataField]
    public ResPath PocketDimensionPath = new ResPath("/Maps/_Goobstation/Nonstations/pocket-dimension.yml");

    /// <summary>
    /// The prototype to spawn for the portal spawned in the pocket dimension.
    /// </summary>
    [DataField]
    public EntProtoId ExitPortalPrototype = "PortalBlue";

    [DataField]
    public SoundSpecifier OpenPortalSound = new SoundPathSpecifier("/Audio/Machines/high_tech_confirm.ogg")
    {
        Params = AudioParams.Default.WithVolume(-2f)
    };

    [DataField]
    public SoundSpecifier ClosePortalSound = new SoundPathSpecifier("/Audio/Machines/button.ogg");
}