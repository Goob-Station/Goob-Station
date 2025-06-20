using Content.Server.Ghost.Components;
using Content.Shared.Ghost;
using Content.Pirate.Shared.CustomGhostSystem;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Server.Player;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Server.CustomGhostSpriteSystem;

public sealed class CustomGhostSpriteSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GhostComponent, PlayerAttachedEvent>(OnShit);
    }

    private void OnShit(EntityUid uid, GhostComponent component, PlayerAttachedEvent args)
    {
        if (!_playerManager.TryGetSessionByEntity(uid, out var session))
            return;

        TrySetCustomSprite(uid, session.Name);
    }


    public void TrySetCustomSprite(EntityUid ghostUid, string ckey)
    {
        var prototypes = _prototypeManager.EnumeratePrototypes<CustomGhostPrototype>();

        foreach (var customGhostPrototype in prototypes)
        {
            if (string.Equals(customGhostPrototype.Ckey, ckey, StringComparison.CurrentCultureIgnoreCase))
            {
                _appearanceSystem.SetData(ghostUid,
                    CustomGhostAppearance.Sprite,
                    customGhostPrototype.CustomSpritePath.ToString());

                if (customGhostPrototype.AlphaOverride > 0)
                {
                    _appearanceSystem.SetData(ghostUid,
                        CustomGhostAppearance.AlphaOverride,
                        customGhostPrototype.AlphaOverride);
                }

                if (customGhostPrototype.GhostName != string.Empty)
                {
                    _metaData.SetEntityName(ghostUid, customGhostPrototype.GhostName);
                    // MetaData(ghostUid).EntityName = customGhostPrototype.GhostName;
                }

                if (customGhostPrototype.GhostDescription != string.Empty)
                {
                    _metaData.SetEntityDescription(ghostUid, customGhostPrototype.GhostDescription);
                    // MetaData(ghostUid).EntityDescription = customGhostPrototype.GhostDescription;
                }

                return;
            }
        }
    }
}
