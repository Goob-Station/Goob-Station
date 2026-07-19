using Content.Goobstation.Client.Particles;
using Content.Goobstation.Shared.GhostCosmetics;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.GhostCosmetics;

public sealed class GhostCosmeticsSystem : EntitySystem
{
    [Dependency] private readonly ParticleSystem _particles = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private readonly Dictionary<EntityUid, (ProtoId<GhostCosmeticPrototype> Cosmetic, ActiveEmitter Emitter)> _emitters = new();

    private enum CosmeticLayers : byte
    {
        Hat,
        Mask,
    }

    public override void Initialize()
    {
        SubscribeLocalEvent<GhostCosmeticsComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GhostCosmeticsComponent, AfterAutoHandleStateEvent>(OnHandleState);
        SubscribeLocalEvent<GhostCosmeticsComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<GhostCosmeticsComponent> ent, ref ComponentStartup args)
    {
        UpdateVisuals(ent);
    }

    private void OnHandleState(Entity<GhostCosmeticsComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateVisuals(ent);
    }

    private void OnShutdown(Entity<GhostCosmeticsComponent> ent, ref ComponentShutdown args)
    {
        RemoveEmitter(ent);

        if (!TryComp(ent, out SpriteComponent? sprite))
            return;

        HideLayer((ent, sprite), CosmeticLayers.Hat);
        HideLayer((ent, sprite), CosmeticLayers.Mask);
    }

    private void UpdateVisuals(Entity<GhostCosmeticsComponent> ent)
    {
        if (TryComp(ent, out SpriteComponent? sprite))
        {
            UpdateLayer((ent, sprite), CosmeticLayers.Hat, ent.Comp.Hat);
            UpdateLayer((ent, sprite), CosmeticLayers.Mask, ent.Comp.Mask);
        }

        UpdateParticles(ent);
    }

    private void UpdateLayer(Entity<SpriteComponent> sprite, CosmeticLayers key, ProtoId<GhostCosmeticPrototype>? cosmetic)
    {
        if (cosmetic is not { } id ||
            !_prototypes.TryIndex(id, out var proto) ||
            proto.Sprite is not { } specifier)
        {
            HideLayer(sprite, key);
            return;
        }

        var layer = _sprite.LayerMapReserve((sprite.Owner, sprite.Comp), key);
        _sprite.LayerSetSprite((sprite.Owner, sprite.Comp), layer, specifier);
        sprite.Comp.LayerSetShader(layer, "unshaded");
        _sprite.LayerSetVisible((sprite.Owner, sprite.Comp), layer, true);
    }

    private void HideLayer(Entity<SpriteComponent> sprite, CosmeticLayers key)
    {
        if (_sprite.LayerMapTryGet((sprite.Owner, sprite.Comp), key, out var layer, false))
            _sprite.LayerSetVisible((sprite.Owner, sprite.Comp), layer, false);
    }

    private void UpdateParticles(Entity<GhostCosmeticsComponent> ent)
    {
        if (_emitters.TryGetValue(ent, out var active))
        {
            if (active.Cosmetic == ent.Comp.Particles)
                return;

            RemoveEmitter(ent);
        }

        if (ent.Comp.Particles is not { } id ||
            !_prototypes.TryIndex(id, out var proto) ||
            proto.ParticleEffect is not { } effect)
        {
            return;
        }

        if (_particles.CreateParticle(effect, ent) is { } emitter)
            _emitters[ent] = (id, emitter);
    }

    private void RemoveEmitter(EntityUid uid)
    {
        if (_emitters.Remove(uid, out var active))
            _particles.RemoveParticle(active.Emitter);
    }
}
