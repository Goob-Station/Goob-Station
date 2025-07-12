// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BeBright <98597725+bebr3ght@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
//
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
            }
            _sprite.LayerSetRsiState((entity, sprite), BorgVisualLayers.Body, prototype.SpriteBodyState);
            _sprite.LayerSetRsiState((entity, sprite), BorgVisualLayers.LightStatus, prototype.SpriteToggleLightState);
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

        if (prototype.SpriteBodyMovementState is { } movementState)
        {
            var spriteMovement = EnsureComp<SpriteMovementComponent>(entity);
            spriteMovement.NoMovementLayers.Clear();
            spriteMovement.NoMovementLayers["movement"] = new PrototypeLayerData
            {
                State = prototype.SpriteBodyState,
            };
            spriteMovement.MovementLayers.Clear();
            spriteMovement.MovementLayers["movement"] = new PrototypeLayerData
            {
                State = movementState,
            };
        }
        else
        {
            RemComp<SpriteMovementComponent>(entity);
        }

        base.UpdateEntityAppearance(entity, prototype, subtypePrototype);
    }
}
