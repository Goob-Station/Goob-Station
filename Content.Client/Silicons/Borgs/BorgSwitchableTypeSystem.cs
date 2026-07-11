// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Movement.Components;
using Content.Shared.Silicons.Borgs;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Client.GameObjects;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Client.Silicons.Borgs;

/// <summary>
/// Client side logic for borg type switching. Sets up primarily client-side visual information.
/// </summary>
/// <seealso cref="SharedBorgSwitchableTypeSystem"/>
/// <seealso cref="BorgSwitchableTypeComponent"/>
public sealed class BorgSwitchableTypeSystem : SharedBorgSwitchableTypeSystem
{
    [Dependency] private readonly BorgSystem _borgSystem = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgSwitchableTypeComponent, AfterAutoHandleStateEvent>(AfterStateHandler);
        SubscribeLocalEvent<BorgSwitchableTypeComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(Entity<BorgSwitchableTypeComponent> ent, ref ComponentStartup args)
    {
        UpdateEntityAppearance(ent);
    }

    private void AfterStateHandler(Entity<BorgSwitchableTypeComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateEntityAppearance(ent);
    }

    protected override void UpdateEntityAppearance(
        Entity<BorgSwitchableTypeComponent> entity,
        BorgTypePrototype prototype,
        BorgSubtypePrototype subtypePrototype)
    {
        var hasMovementState = prototype.SpriteBodyMovementState is null;

        if (TryComp(entity, out SpriteComponent? sprite))
        {
            _sprite.LayerSetRsiState((entity, sprite), BorgVisualLayers.Body, prototype.SpriteBodyState);
            _sprite.LayerSetRsiState((entity, sprite), BorgVisualLayers.Light, prototype.SpriteBodyState);
            _sprite.LayerSetRsiState((entity, sprite), BorgVisualLayers.LightStatus, prototype.SpriteBodyState);

            var rsiPath = SpriteSpecifierSerializer.TextureRoot / subtypePrototype.SpritePath;
            if (_resourceCache.TryGetResource<RSIResource>(rsiPath, out var resource))
            {
                _sprite.LayerSetRsi((entity, sprite), BorgVisualLayers.Body, resource.RSI);
                _sprite.LayerSetRsi((entity, sprite), BorgVisualLayers.Light, resource.RSI);
                _sprite.LayerSetRsi((entity, sprite), BorgVisualLayers.LightStatus, resource.RSI);

                if (prototype.SpriteBodyMovementState is { } movementState)
                    hasMovementState = resource.RSI.TryGetState(movementState, out _);
            }
            _sprite.LayerSetRsiState((entity, sprite), BorgVisualLayers.Body, prototype.SpriteBodyState);
            _sprite.LayerSetRsiState((entity, sprite), BorgVisualLayers.LightStatus, prototype.SpriteToggleLightState);
            _sprite.SetScale((entity, sprite), prototype.SpriteScale); // Pirate: source quadborg art uses a custom sprite scale.
            sprite.NoRotation = prototype.SpriteNoRotation; // Pirate: source quadborg art uses directional states.
        }

        if (TryComp(entity, out BorgChassisComponent? chassis))
        {
            _borgSystem.SetMindStates(
                (entity.Owner, chassis),
                prototype.SpriteHasMindState,
                prototype.SpriteNoMindState);

            if (TryComp(entity, out AppearanceComponent? appearance))
            {
                // Queue update so state changes apply.
                _appearance.QueueUpdate(entity, appearance);
            }
        }

        base.UpdateEntityAppearance(entity, prototype, subtypePrototype); // Goob pass along subtypePrototype. No i don't know what the fuck happened here and im too tired to care.

        // Pirate: Custom subtype RSIs may only have an idle body state.
        if (!hasMovementState &&
            TryComp(entity, out SpriteMovementComponent? movement) &&
            movement.MovementLayers.TryGetValue("movement", out var movementLayer))
        {
            movementLayer.State = prototype.SpriteBodyState;
        }
    }
}
