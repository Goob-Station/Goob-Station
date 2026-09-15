// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Procedural.Components;

namespace Content.Server._Oskarrr.WildLands;

/// <summary>
/// DISABLED: jockey shuttle + aborigine camp placement was hanging / blocking map init.
/// Re-enable body of <see cref="OnLavalandMapInit"/> when needed.
/// </summary>
public sealed class WildLandsOpposingCampsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LavalandMapComponent, MapInitEvent>(OnLavalandMapInit);
    }

    private void OnLavalandMapInit(Entity<LavalandMapComponent> ent, ref MapInitEvent args)
    {
        // Spawns disabled — do not load ruin_jockey_shuttle or SpawnerWildLandsTribeCamp.
    }
}
