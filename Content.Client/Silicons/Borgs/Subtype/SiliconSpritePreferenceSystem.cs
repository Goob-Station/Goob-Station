using Content.Shared.Silicons.Borgs;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Client.GameObjects;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Client.Silicons.Borgs.Subtype;

public sealed partial class SiliconSpritePreferenceSystem : SharedBorgSwitchableSubtypeSystem
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgSwitchableSubtypeComponent, BorgSubtypeChangedEvent>(OnSubtypeChanged);
    }
    private void OnSubtypeChanged(Entity<BorgSwitchableSubtypeComponent> ent, ref BorgSubtypeChangedEvent args)
    {
        SetAppearanceFromSubtype(ent, args.Subtype);
    }

    protected override void SetAppearanceFromSubtype(
        Entity<BorgSwitchableSubtypeComponent> ent,
        ProtoId<BorgSubtypePrototype> subtype)
    {
        if (!_prototypeManager.TryIndex(subtype, out var subtypePrototype))
            return;
        if (!_prototypeManager.TryIndex(subtypePrototype.ParentBorgType, out var borgType))
            return;

        if (!TryComp(ent, out SpriteComponent? sprite))
            return;

        var rsiPath = SpriteSpecifierSerializer.TextureRoot / subtypePrototype.SpritePath;

        if (_resourceCache.TryGetResource<RSIResource>(rsiPath, out var resource))
            sprite.BaseRSI = resource.RSI;
    }
}
