using Content.Goobstation.Shared.MantisBlades;
using Content.Shared.Body.Part;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.MantisBlades;

public sealed partial class MantisBladeVisualsSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private IGameTiming _timing = default!;

    /// <summary>
    /// Users whose blades could not be resolved yet (not received / not attached to an arm yet).
    /// </summary>
    private readonly HashSet<EntityUid> _unresolved = new();

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
        _unresolved.Remove(ent);

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
            if (user.IsExtended && (user.AnimationPlayed != IsAnimationPlayed(user) || _unresolved.Contains(uid)))
                UpdateLayers((uid, user), sprite);
        }
    }

    private bool IsAnimationPlayed(MantisBladeUserComponent user)
        => _timing.CurTime >= user.ExtendedAt + user.AnimationTime;

    private void UpdateLayers(Entity<MantisBladeUserComponent> ent, SpriteComponent sprite)
    {
        var left = false;
        var right = false;
        var unresolved = false;

        if (ent.Comp.IsExtended)
            foreach (var blade in ent.Comp.Blades)
            {
                // Blades is an auto-networked entity list: on the client an entry can be EntityUid.Invalid
                // (blade not received through PVS yet, or deleted), so don't use Transform() here.
                // Its arm may also not be attached yet; retry from FrameUpdate until it resolves.
                if (!TryComp(blade, out TransformComponent? bladeXform)
                    || !TryComp<BodyPartComponent>(bladeXform.ParentUid, out var part))
                {
                    unresolved = true;
                    continue;
                }

                if (part.Symmetry == BodyPartSymmetry.Left)
                    left = true;
                else
                    right = true;
            }

        if (unresolved)
            _unresolved.Add(ent);
        else
            _unresolved.Remove(ent);

        ent.Comp.AnimationPlayed = IsAnimationPlayed(ent.Comp);
        var prefix = ent.Comp.AnimationPlayed ? "inhand" : "popout-inhand";

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
