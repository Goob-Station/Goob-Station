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

        var random = new Random();
        foreach (var customGhostPrototype in prototypes)
        {
            if (string.Equals(customGhostPrototype.Ckey, ckey, StringComparison.CurrentCultureIgnoreCase))
            {
                string spriteData;
                var chosenRsi = customGhostPrototype.CustomSpritePath.Count > 1
                    ? customGhostPrototype.CustomSpritePath[random.Next(customGhostPrototype.CustomSpritePath.Count)]
                    : customGhostPrototype.CustomSpritePath[0];
                string? chosenState = null;
                try
                {
                    var metaPath = System.IO.Path.Combine(chosenRsi.ToString(), "meta.json");
                    string? altMetaPath = null;
                    string? altMetaPathTextures = null;
                    if (!System.IO.File.Exists(metaPath))
                    {
                        altMetaPath = System.IO.Path.Combine("Resources", metaPath.Replace('/', System.IO.Path.DirectorySeparatorChar));
                        altMetaPathTextures = System.IO.Path.Combine("Resources", "Textures", metaPath.Replace('/', System.IO.Path.DirectorySeparatorChar));
                    }
                    string? usedMetaPath = null;
                    if (System.IO.File.Exists(metaPath))
                        usedMetaPath = metaPath;
                    else if (!string.IsNullOrEmpty(altMetaPath) && System.IO.File.Exists(altMetaPath))
                        usedMetaPath = altMetaPath;
                    else if (!string.IsNullOrEmpty(altMetaPathTextures) && System.IO.File.Exists(altMetaPathTextures))
                        usedMetaPath = altMetaPathTextures;
                    if (!string.IsNullOrEmpty(usedMetaPath))
                    {
                        var metaJson = System.IO.File.ReadAllText(usedMetaPath);
                        var meta = System.Text.Json.JsonDocument.Parse(metaJson);
                        if (meta.RootElement.TryGetProperty("states", out var statesElem) && statesElem.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            var states = new List<string>();
                            foreach (var e in statesElem.EnumerateArray())
                            {
                                if (e.ValueKind == System.Text.Json.JsonValueKind.Object && e.TryGetProperty("name", out var nameProp))
                                {
                                    var s = nameProp.GetString();
                                    if (!string.IsNullOrEmpty(s))
                                        states.Add(s);
                                }
                                else if (e.ValueKind == System.Text.Json.JsonValueKind.String)
                                {
                                    var s = e.GetString();
                                    if (!string.IsNullOrEmpty(s))
                                        states.Add(s);
                                }
                            }
                            if (states.Count > 0)
                            {
                                chosenState = states[random.Next(states.Count)];
                            }
                        }
                    }
                }
                catch
                {
                }
                spriteData = chosenRsi.ToString();
                if (!string.IsNullOrEmpty(chosenState))
                    spriteData += $":{chosenState}";

                _appearanceSystem.SetData(ghostUid,
                    CustomGhostAppearance.Sprite,
                    spriteData);

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
