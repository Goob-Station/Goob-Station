using Content.Goobstation.Shared.Gangwars.Components;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Gangwars;

/// <summary>
/// applies the components visibility directly to the sprite's alpha channel,
/// leaving the RGB color untouched so another system (e.g. gang color) can manage it.
/// Restores full opacity when the component is removed.
/// </summary>
public sealed class StealthPreserveColorSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StealthPreserveColorComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<StealthPreserveColorComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<StealthPreserveColorComponent, AfterAutoHandleStateEvent>(OnState);
    }

    private void OnStartup(Entity<StealthPreserveColorComponent> ent, ref ComponentStartup args)
        => SetAlpha(ent, Math.Clamp(ent.Comp.Visibility, 0f, 1f));

    private void OnState(Entity<StealthPreserveColorComponent> ent, ref AfterAutoHandleStateEvent args)
        => SetAlpha(ent, Math.Clamp(ent.Comp.Visibility, 0f, 1f));

    private void OnShutdown(Entity<StealthPreserveColorComponent> ent, ref ComponentShutdown args)
    {
        if (!TerminatingOrDeleted(ent.Owner))
            SetAlpha(ent, 1f);
    }

    private void SetAlpha(EntityUid uid, float alpha)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        var color = sprite.Color;
        _sprite.SetColor((uid, sprite), new Color(color.R, color.G, color.B, alpha));
    }
}
