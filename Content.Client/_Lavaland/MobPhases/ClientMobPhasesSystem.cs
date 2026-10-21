using Content.Goobstation.Common.Sprite;
using Content.Shared._Lavaland.MobPhases;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Client._Lavaland.MobPhases;

public sealed partial class ClientMobPhasesSystem : MobPhasesSystem
{
    [Dependency] private SpriteSystem _sprite = default!;

    [SubscribeLocalEvent]
    private void OnPhaseChanged(Entity<MobPhasesComponent> ent, ref MobPhaseChangedEvent args)
    {
        if (!TryGetPhase(ent.Comp, args.NewPhase, out var phase))
            return;

        var held = new SpriteHeldEvent();
        RaiseLocalEvent(ent, ref held);

        if (!held.Held)
            ApplyPhaseSprite(ent, phase);
    }

    [SubscribeLocalEvent]
    private void OnSpriteReleased(Entity<MobPhasesComponent> ent, ref SpriteReleasedEvent args)
    {
        if (TryGetPhase(ent.Comp, ent.Comp.CurrentPhase, out var phase))
            ApplyPhaseSprite(ent, phase);
    }

    private void ApplyPhaseSprite(Entity<MobPhasesComponent> ent, MobPhase phase)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        sprite.EnableDirectionOverride = false;

        foreach (var (index, original) in ent.Comp.OriginalSprites)
            if (_sprite.LayerExists((ent.Owner, sprite), index))
                _sprite.LayerSetSprite((ent.Owner, sprite), index, original);

        ent.Comp.OriginalSprites.Clear();

        if (phase.NewSprite is not { } specifier)
            return;

        var layer = 0;
        if (phase.Layer is { } key && !_sprite.LayerMapTryGet((ent.Owner, sprite), key, out layer, true)
            || !_sprite.TryGetLayer((ent.Owner, sprite), layer, out var current, true)
            || current.ActualRsi is not { } rsi
            || current.State.Name is not { } state)
            return;

        ent.Comp.OriginalSprites[layer] = new SpriteSpecifier.Rsi(rsi.Path, state);
        _sprite.LayerSetSprite((ent.Owner, sprite), layer, specifier);
    }
}
