using Content.Goobstation.Shared.MantisBlades;
using Content.Shared.Body.Part;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.MantisBlades;

public sealed class MantisBladeVisualsSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private enum BladeLayerKey : byte
    {
        Left,
        Right,
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MantisBladeUserComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MantisBladeUserComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<MantisBladeUserComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    private void OnStartup(Entity<MantisBladeUserComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        foreach (var key in new[] { BladeLayerKey.Left, BladeLayerKey.Right })
        {
            if (_sprite.LayerMapTryGet((ent, sprite), key, out _, false))
                continue;

            var layer = _sprite.AddLayer((ent, sprite), new SpriteSpecifier.Rsi(ent.Comp.Rsi, "inhand-left"));
            _sprite.LayerMapSet((ent, sprite), key, layer);
        }

        UpdateLayers(ent, sprite);
    }

    private void OnRemove(Entity<MantisBladeUserComponent> ent, ref ComponentRemove args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        _sprite.RemoveLayer((ent, sprite), BladeLayerKey.Left);
        _sprite.RemoveLayer((ent, sprite), BladeLayerKey.Right);
    }

    private void OnHandleState(Entity<MantisBladeUserComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (TryComp<SpriteComponent>(ent, out var sprite))
            UpdateLayers(ent, sprite);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var query = EntityQueryEnumerator<MantisBladeUserComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var user, out var sprite))
        {
            if (user.Extended && user.Popped != IsPopped(user))
                UpdateLayers((uid, user), sprite);
        }
    }

    private bool IsPopped(MantisBladeUserComponent user)
        => _timing.CurTime >= user.ExtendedAt + user.PopTime;

    private void UpdateLayers(Entity<MantisBladeUserComponent> ent, SpriteComponent sprite)
    {
        var left = false;
        var right = false;

        if (ent.Comp.Extended)
            foreach (var blade in ent.Comp.Blades)
            {
                if (!TryComp<BodyPartComponent>(Transform(blade).ParentUid, out var part))
                    continue;

                if (part.Symmetry == BodyPartSymmetry.Left)
                    left = true;
                else
                    right = true;
            }

        ent.Comp.Popped = IsPopped(ent.Comp);
        var prefix = ent.Comp.Popped ? "inhand" : "popout-inhand";

        SetLayer(BladeLayerKey.Left, $"{prefix}-left", left);
        SetLayer(BladeLayerKey.Right, $"{prefix}-right", right);
        return;

        void SetLayer(BladeLayerKey key, string state, bool visible)
        {
            if (!_sprite.LayerMapTryGet((ent, sprite), key, out var layer, false))
                return;

            _sprite.LayerSetRsiState((ent, sprite), layer, state);
            _sprite.LayerSetVisible((ent, sprite), layer, visible);
        }
    }
}
