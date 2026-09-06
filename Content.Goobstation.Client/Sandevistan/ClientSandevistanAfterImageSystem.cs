using Content.Goobstation.Shared.Sandevistan;
using Content.Shared.Tag;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using DrawDepthEnum = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Goobstation.Client.Sandevistan;

public sealed class ClientSandevistanAfterimageSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;

    private static readonly ProtoId<TagPrototype> HideContextMenuTag = "HideContextMenu";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanAfterimageComponent, ComponentStartup>(OnAfterimageStartup);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var query = EntityQueryEnumerator<SandevistanAfterimageComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.DespawnAt is not { } despawnAt)
            {
                var source = comp.SourceEntity;
                if (TerminatingOrDeleted(source) || Paused(source) || !HasComp<ActiveSandevistanUserComponent>(source))
                    comp.DespawnAt = _timing.CurTime + comp.FadeDuration;
                continue;
            }

            if (_timing.CurTime >= despawnAt)
            {
                QueueDel(uid);
                continue;
            }

            var remaining = (float) (despawnAt - _timing.CurTime).TotalSeconds;
            var alpha = Math.Clamp(remaining / (float) comp.FadeDuration.TotalSeconds, 0f, 1f) * comp.BaseAlpha;
            if (TryComp<SpriteComponent>(uid, out var sprite))
                _sprite.SetColor((uid, sprite), comp.Color.WithAlpha(alpha));
        }
    }

    private void OnAfterimageStartup(Entity<SandevistanAfterimageComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent.Comp.SourceEntity, out var userSprite))
            return;

        _tagSystem.AddTag(ent, HideContextMenuTag);

        var afterimageSprite = EnsureComp<SpriteComponent>(ent);
        _sprite.CopySprite((ent.Comp.SourceEntity, userSprite), (ent.Owner, afterimageSprite));
        _sprite.SetDrawDepth((ent.Owner, afterimageSprite), (int) DrawDepthEnum.FloorEffects);
        _sprite.SetColor((ent.Owner, afterimageSprite), ent.Comp.Color.WithAlpha(ent.Comp.BaseAlpha));
        afterimageSprite.PostShader = null;

        var layer = 0;
        foreach (var _ in afterimageSprite.AllLayers)
            afterimageSprite.LayerSetShader(layer++, "unshaded");
        afterimageSprite.RenderOrder = (uint) (int.MaxValue - ent.Comp.Order);
        afterimageSprite.EnableDirectionOverride = true;
        afterimageSprite.DirectionOverride = ent.Comp.DirectionOverride;
    }
}
