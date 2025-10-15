// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Princess Cheeseballs <66055347+Pronana@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     When a mob is walking should its X / Y movement be relative to its parent (true) or the map (false).
    /// </summary>
    public static readonly CVarDef<bool> RelativeMovement =
        CVarDef.Create("physics.relative_movement", true, CVar.ARCHIVE | CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<float> MinFriction =
        CVarDef.Create("physics.min_friction", 0.0f, CVar.ARCHIVE | CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<float> AirFriction =
        CVarDef.Create("physics.air_friction", 0.2f, CVar.ARCHIVE | CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<float> OffgridFriction =
        CVarDef.Create("physics.offgrid_friction", 0.0f, CVar.ARCHIVE | CVar.REPLICATED | CVar.SERVER); // Goobstation

    public static readonly CVarDef<float> TileFrictionModifier =
        CVarDef.Create("physics.tile_friction", 8.0f, CVar.ARCHIVE | CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<float> StopSpeed =
        CVarDef.Create("physics.stop_speed", 0.1f, CVar.ARCHIVE | CVar.REPLICATED | CVar.SERVER);
}
