// SPDX-License-Identifier: AGPL-3.0-or-later

using System.IO;
using Content.Shared._Oskarrr.CustomGhost;
using Content.Shared.Ghost;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Oskarrr.CustomGhost;

public sealed class CustomGhostSpriteSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
    }

    private void OnPlayerAttached(PlayerAttachedEvent args)
    {
        if (!HasComp<GhostComponent>(args.Entity))
            return;

        TrySetCustomSprite(args.Entity, args.Player.Name);
    }

    public bool TrySetCustomSprite(EntityUid ghostUid, string ckey)
    {
        var prototypes = _prototypeManager.EnumeratePrototypes<CustomGhostPrototype>();
        var random = new Random();

        foreach (var customGhostPrototype in prototypes)
        {
            if (!string.Equals(customGhostPrototype.Ckey, ckey, StringComparison.CurrentCultureIgnoreCase))
                continue;

            var chosenRsi = customGhostPrototype.CustomSpritePath.Count > 1
                ? customGhostPrototype.CustomSpritePath[random.Next(customGhostPrototype.CustomSpritePath.Count)]
                : customGhostPrototype.CustomSpritePath[0];

            string? chosenState = null;
            try
            {
                var metaPath = Path.Combine(chosenRsi.ToString(), "meta.json");
                string? altMetaPath = null;
                string? altMetaPathTextures = null;
                if (!File.Exists(metaPath))
                {
                    altMetaPath = Path.Combine("Resources", metaPath.Replace('/', Path.DirectorySeparatorChar));
                    altMetaPathTextures = Path.Combine("Resources", "Textures", metaPath.Replace('/', Path.DirectorySeparatorChar));
                }

                string? usedMetaPath = null;
                if (File.Exists(metaPath))
                    usedMetaPath = metaPath;
                else if (!string.IsNullOrEmpty(altMetaPath) && File.Exists(altMetaPath))
                    usedMetaPath = altMetaPath;
                else if (!string.IsNullOrEmpty(altMetaPathTextures) && File.Exists(altMetaPathTextures))
                    usedMetaPath = altMetaPathTextures;

                if (!string.IsNullOrEmpty(usedMetaPath))
                {
                    var metaJson = File.ReadAllText(usedMetaPath);
                    var meta = System.Text.Json.JsonDocument.Parse(metaJson);
                    if (meta.RootElement.TryGetProperty("states", out var statesElem) &&
                        statesElem.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        var states = new List<string>();
                        foreach (var e in statesElem.EnumerateArray())
                        {
                            if (e.ValueKind == System.Text.Json.JsonValueKind.Object &&
                                e.TryGetProperty("name", out var nameProp))
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
                            chosenState = states[random.Next(states.Count)];
                    }
                }
            }
            catch
            {
                // Ignore meta parse failures; RSI path alone still works.
            }

            var spriteData = chosenRsi.ToString();
            if (!string.IsNullOrEmpty(chosenState))
                spriteData += $":{chosenState}";

            _appearanceSystem.SetData(ghostUid, CustomGhostAppearance.Sprite, spriteData);
            _appearanceSystem.SetData(ghostUid, CustomGhostAppearance.MaxSize, customGhostPrototype.MaxSize);

            if (customGhostPrototype.AlphaOverride > 0)
            {
                _appearanceSystem.SetData(ghostUid,
                    CustomGhostAppearance.AlphaOverride,
                    customGhostPrototype.AlphaOverride);
            }

            if (customGhostPrototype.GhostName != string.Empty)
                _metaData.SetEntityName(ghostUid, customGhostPrototype.GhostName);

            if (customGhostPrototype.GhostDescription != string.Empty)
                _metaData.SetEntityDescription(ghostUid, customGhostPrototype.GhostDescription);

            return true;
        }

        return false;
    }
}
