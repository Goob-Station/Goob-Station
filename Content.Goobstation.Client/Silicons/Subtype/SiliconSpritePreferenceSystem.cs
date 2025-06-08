// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Silicons.Borgs;
using Content.Shared.Silicons.Borgs;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Client.GameObjects;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Goobstation.Client.Silicons.Subtype;

public sealed partial class SiliconSpritePreferenceSystem : SharedBorgSwitchableSubtypeSystem
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgSwitchableSubtypeComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<BorgSwitchableSubtypeComponent, AfterAutoHandleStateEvent>(AfterStateHandler);
    }
    private void OnComponentStartup(Entity<BorgSwitchableSubtypeComponent> ent, ref ComponentStartup args) =>
        UpdateVisuals(ent);

    private void AfterStateHandler(Entity<BorgSwitchableSubtypeComponent> ent, ref AfterAutoHandleStateEvent args) =>
        UpdateVisuals(ent);

    protected override void SetAppearanceFromSubtype(
        Entity<BorgSwitchableSubtypeComponent> ent,
        ProtoId<BorgSubtypePrototype> subtype)
    {
        if (!_prototypeManager.TryIndex(subtype, out var subtypePrototype)
            || !_prototypeManager.TryIndex(subtypePrototype.ParentBorgType, out var borgType)
            || !TryComp(ent, out SpriteComponent? sprite))
            return;

        var rsiPath = SpriteSpecifierSerializer.TextureRoot / subtypePrototype.SpritePath;

        if (_resourceCache.TryGetResource<RSIResource>(rsiPath, out var resource))
        {
            // what the fuck
            subtypePrototype.SpriteBodyState = borgType.SpriteBodyState;
            subtypePrototype.SpriteToggleLightState = borgType.SpriteToggleLightState;
            subtypePrototype.SpriteHasMindState = borgType.SpriteHasMindState;
            subtypePrototype.SpriteNoMindState = borgType.SpriteNoMindState;

            if (!_appearance.TryGetData<bool>(ent, BorgVisuals.HasPlayer, out var hasPlayer))
                hasPlayer = false;
            sprite.LayerSetState(BorgVisualLayers.Body, subtypePrototype.SpriteBodyState);
            sprite.LayerSetState(BorgVisualLayers.Light, hasPlayer ? subtypePrototype.SpriteHasMindState : subtypePrototype.SpriteNoMindState);
            sprite.LayerSetState(BorgVisualLayers.LightStatus, subtypePrototype.SpriteToggleLightState);

            sprite.LayerSetRSI(BorgVisualLayers.Body.GetHashCode(), resource.RSI);
            sprite.LayerSetRSI(BorgVisualLayers.Light.GetHashCode(), resource.RSI);
            sprite.LayerSetRSI(BorgVisualLayers.LightStatus.GetHashCode(), resource.RSI);
        }
    }
}
