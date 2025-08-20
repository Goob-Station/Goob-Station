using Content.Server.Ghost.Components;
using Content.Shared.Ghost;
using Content.Shared._White.CustomGhostSystem;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._White.CustomGhostSpriteSystem;

public sealed class CustomGhostSpriteSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GhostComponent, PlayerAttachedEvent>(OnChangeAppearance);
    }

    private void OnChangeAppearance(EntityUid uid, GhostComponent component, PlayerAttachedEvent args)
    {
        if(!_playerManager.TryGetSessionByEntity(uid, out var session))
            return;

        TrySetCustomSprite(uid, session.Name);
    }

    public void TrySetCustomSprite(EntityUid ghostUid, string ckey)
    {
        var prototypes = _prototypeManager.EnumeratePrototypes<CustomGhostPrototype>();

        foreach (var customGhostPrototype in prototypes)
        {
            if (!string.Equals(customGhostPrototype.Ckey, ckey, StringComparison.CurrentCultureIgnoreCase))
                continue;
            _appearanceSystem.SetData(ghostUid, CustomGhostAppearance.Sprite, customGhostPrototype.CustomSpritePath.ToString());
            _appearanceSystem.SetData(ghostUid, CustomGhostAppearance.SizeOverride, customGhostPrototype.SizeOverride);

            if (customGhostPrototype.AlphaOverride > 0)
            {
                _appearanceSystem.SetData(ghostUid, CustomGhostAppearance.AlphaOverride, customGhostPrototype.AlphaOverride);
            }

            if (customGhostPrototype.GhostName != string.Empty)
            {
                _metaData.SetEntityName(ghostUid, customGhostPrototype.GhostName);
            }

            if (customGhostPrototype.GhostDescription != string.Empty)
            {
                _metaData.SetEntityDescription(ghostUid, customGhostPrototype.GhostDescription);
            }

            return;
        }
    }
} 
