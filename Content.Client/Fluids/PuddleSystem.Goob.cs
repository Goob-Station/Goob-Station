using Content.Goobstation.Common.Footprints;
using Content.Shared._ES.Fluids;
using Content.Shared.Fluids;
using Content.Shared.Random.Helpers;
using Robust.Client.GameObjects;
using Robust.Shared.Random;

namespace Content.Client.Fluids;

public sealed partial class PuddleSystem
{
    private readonly IRobustRandom _puddleRandom = new RobustRandom();

    private void UpdatePuddleSprite(Entity<SpriteComponent> ent, float volume, IReadOnlyDictionary<Enum, object> appearanceData)
    {
        if (HasComp<FootprintComponent>(ent))
        {
            if (appearanceData.TryGetValue(PuddleVisuals.SolutionColor, out var footprintColor))
                _sprite.SetColor(ent.AsNullable(), ((Color) footprintColor).WithAlpha(0.7f));
            return;
        }

        var spriteSetId = DefaultPuddleSpriteSet;
        if (appearanceData.TryGetValue(PuddleVisuals.SpriteSet, out var spriteSetObj) && spriteSetObj is string spriteSetString)
            spriteSetId = spriteSetString;

        if (!ProtoMan.TryIndex(spriteSetId, out var spriteSet) && !ProtoMan.TryIndex(DefaultPuddleSpriteSet, out spriteSet))
            return;

        var (sprites, size) = volume switch
        {
            < LowThreshold => (spriteSet.SmallSprites, 0),
            < MediumThreshold => (spriteSet.MediumSprites, 1),
            _ => (spriteSet.LargeSprites, 2),
        };

        if (sprites.Count > 0)
        {
            _puddleRandom.SetSeed(SharedRandomExtensions.HashCodeCombine(GetNetEntity(ent).Id, size));
            _sprite.LayerSetSprite(ent.AsNullable(), 0, _puddleRandom.Pick(sprites));
            _sprite.LayerSetRotation(ent.AsNullable(), 0, _puddleRandom.NextAngle().RoundToCardinalAngle());
        }

        var baseColor = spriteSet.BaseColor;

        if (spriteSet.Recolor && appearanceData.TryGetValue(PuddleVisuals.SolutionColor, out var colorObj))
            _sprite.SetColor(ent.AsNullable(), (Color) colorObj * baseColor);
        else
            _sprite.SetColor(ent.AsNullable(), baseColor);
    }
}
