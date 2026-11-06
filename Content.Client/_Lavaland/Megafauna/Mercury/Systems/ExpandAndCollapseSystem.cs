using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Robust.Client.GameObjects;
using System.Numerics;

namespace Content.Client._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Expand the sprite size, and then once it reaches the maximum size, return it to the initial size.
/// </summary>
public sealed class ExpandAndCollapseSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ExpandAndCollapseComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, ExpandAndCollapseComponent comp, MapInitEvent args)
    {
        comp.CurrentScale = comp.StartingScale;
        comp.Accumulator = 0f;
        comp.Collapsing = false;

        comp.ExpandTime = comp.ExpandTime * 10; // for some ungodly reason this shit runs 10 times fast than it should, so we secretly run this here
        comp.CollapseTime = comp.CollapseTime * 10; // don't ask me why, I have no fucking clue

        if (TryComp<SpriteComponent>(uid, out var sprite))
        {
            sprite.Scale = new Vector2(comp.CurrentScale, comp.CurrentScale);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ExpandAndCollapseComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var comp, out var sprite))
        {
            if (comp.Collapsing && comp.Accumulator >= comp.CollapseTime)
                continue;

            comp.Accumulator += frameTime;

            if (!comp.Collapsing)
            {
                // expand
                var expandProgress = MathF.Min(comp.Accumulator / comp.ExpandTime, 1f);
                comp.CurrentScale = comp.StartingScale + (comp.MaxScale - comp.StartingScale) * expandProgress;

                if (expandProgress >= 1f)
                {
                    comp.Collapsing = true;
                    comp.Accumulator = 0f;
                }
            }
            else
            {
                // collapse
                var collapseProgress = MathF.Min(comp.Accumulator / comp.CollapseTime, 1f);
                comp.CurrentScale = MathF.Max(comp.MaxScale * (1f - collapseProgress), comp.StartingScale);
            }

            sprite.Scale = new Vector2(comp.CurrentScale, comp.CurrentScale);
        }
    }
}
