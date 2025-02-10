using Content.Client.Clickable;
using Content.Client.Interactable.Components;
using Content.Shared._Goobstation.MULE;
using Content.Shared._Goobstation.MULE.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.Verbs;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;
using Robust.Client.GameObjects;

namespace Content.Client._Goobstation.MULE;

/// <summary>
/// This handles...
/// </summary>
public sealed class ShowOnBuckle : EntitySystem
{
    [Dependency] public required EntityManager _entityManager = default!;
    private DrawDepth _drawDepth;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ShowOnBuckleComponent, StrappedEvent>(OnStrap);
        SubscribeLocalEvent<ShowOnBuckleComponent, UnstrappedEvent>(OnUnstrap);
    }

    private void OnUnstrap(EntityUid uid, ShowOnBuckleComponent component, ref UnstrappedEvent args)
    {
        var spriteComponent = _entityManager.GetComponent<SpriteComponent>(args.Buckle.Owner);
        if(!_entityManager.HasComponent<ClickableComponent>(args.Buckle.Owner))
            _entityManager.AddComponent<ClickableComponent>(args.Buckle.Owner);
        if(!_entityManager.HasComponent<InteractionOutlineComponent>(args.Buckle.Owner))
            _entityManager.AddComponent<InteractionOutlineComponent>(args.Buckle.Owner);

        spriteComponent.DrawDepth = (int) _drawDepth;
        _drawDepth = default;
    }
    private void OnStrap(EntityUid uid, ShowOnBuckleComponent component, ref StrappedEvent args)
    {
        var spriteComponent = _entityManager.GetComponent<SpriteComponent>(args.Buckle.Owner);
        if (_entityManager.HasComponent<ClickableComponent>(args.Buckle.Owner))
            _entityManager.RemoveComponent<ClickableComponent>(args.Buckle.Owner);
        if(_entityManager.HasComponent<InteractionOutlineComponent>(args.Buckle.Owner))
            _entityManager.RemoveComponent<InteractionOutlineComponent>(args.Buckle.Owner);
        _drawDepth = (DrawDepth) spriteComponent.DrawDepth;
        spriteComponent.DrawDepth = (int) DrawDepth.OverMobs;
    }
}
