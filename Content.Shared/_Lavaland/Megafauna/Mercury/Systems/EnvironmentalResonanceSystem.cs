using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Events;
using System.Numerics;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Spawns an entity in two corners of the screen, then skips a tile in a specific direction and does it again.
/// Which corners of the screen are decided by a bool.
/// </summary>
public sealed class EnvironmentalResonanceSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EnvironmentalResonanceComponent, EnvironmentalResonanceActionEvent>(OnResonance);
    }

    private void OnResonance(EntityUid uid, EnvironmentalResonanceComponent comp, EnvironmentalResonanceActionEvent args)
    {
        if (args.Handled)
            return;

        var coords = Transform(uid).Coordinates;

        // The system is fairly simple but basically it spawns two entities, one at each corner of the screen
        // Then it skips a tile down, and spawns it again, over and over.
        // Npt very useful unless the entity it spawns is moving, so I don't expect much use from this system.
        if (!args.Vertical)
        {
            for (int i = 0; i < comp.RowNumber; i++)
            {
                float yOffset = comp.VerticalOffset - (i * comp.TileSkip);
                var coordsNW = coords.Offset(new Vector2(-comp.HorizontalOffset, yOffset));
                var coordsNE = coords.Offset(new Vector2(comp.HorizontalOffset, yOffset));

                PredictedSpawnAtPosition(comp.RightPrototype, coordsNW, null);
                //PredictedSpawnAtPosition(comp.LeftPrototype, coordsNE, null); // Mispredicted spawning leads to misalignment. Potentially too busy
            }
        }
        else
        {
            for (int i = 0; i < comp.RowNumber; i++)
            {
                float xOffset = -comp.HorizontalOffset + (i * comp.TileSkip);
                var coordsNW = coords.Offset(new Vector2(xOffset, comp.VerticalOffset));
                var coordsSW = coords.Offset(new Vector2(xOffset, -comp.VerticalOffset));

                PredictedSpawnAtPosition(comp.DownPrototype, coordsNW, null);
                //PredictedSpawnAtPosition(comp.UpPrototype, coordsSW, null); // Mispredicted spawning leads to misalignment. Potentially too busy
            }
        }
        args.Handled = true;
    }
}
