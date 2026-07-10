// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Humanoid.Markings
{
    public sealed class MarkingManager
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        private readonly List<MarkingPrototype> _index = new();
        public FrozenDictionary<MarkingCategories, FrozenDictionary<string, MarkingPrototype>> CategorizedMarkings = default!;
        public FrozenDictionary<string, MarkingPrototype> Markings = default!;

        public void Initialize()
        {
            _prototypeManager.PrototypesReloaded += OnPrototypeReload;
            CachePrototypes();
        }
        
        // Pirate changes start
        /// <summary>
        /// Filters a list of markings, returning only those that are valid for the given player.
        /// </summary>
        public List<Marking> FilterValidMarkings(List<Marking> markings, string species, Sex sex, string ckey)
        {
            var validMarkings = new List<Marking>();
            foreach (var marking in markings)
            {
                if (CanBeApplied(species, sex, marking, _prototypeManager, ckey))
                {
                    validMarkings.Add(marking);
                }
            }
            return validMarkings;
        }
        // Pirate changes end

        private void CachePrototypes()
        {
            _index.Clear();
            var markingDict = new Dictionary<MarkingCategories, Dictionary<string, MarkingPrototype>>();

            foreach (var category in Enum.GetValues<MarkingCategories>())
            {
                markingDict.Add(category, new());
            }

            foreach (var prototype in _prototypeManager.EnumeratePrototypes<MarkingPrototype>())
            {
                _index.Add(prototype);
                markingDict[prototype.MarkingCategory].Add(prototype.ID, prototype);
            }

            Markings = _prototypeManager.EnumeratePrototypes<MarkingPrototype>().ToFrozenDictionary(x => x.ID);
            CategorizedMarkings = markingDict.ToFrozenDictionary(
                x => x.Key,
                x => x.Value.ToFrozenDictionary());
        }

        public FrozenDictionary<string, MarkingPrototype> MarkingsByCategory(MarkingCategories category)
        {
            // all marking categories are guaranteed to have a dict entry
            return CategorizedMarkings[category];
        }

        /// <summary>
        ///     Markings by category and species.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="species"></param>
        /// <remarks>
        ///     This is done per category, as enumerating over every single marking by species isn't useful.
        ///     Please make a pull request if you find a use case for that behavior.
        /// </remarks>
        /// <returns></returns>
        public IReadOnlyDictionary<string, MarkingPrototype> MarkingsByCategoryAndSpecies(MarkingCategories category,
            string species, string? ckey = null) // Pirate ckey for restricted players
        {
            var speciesProto = _prototypeManager.Index<SpeciesPrototype>(species);
            var markingPoints = _prototypeManager.Index(speciesProto.MarkingPoints);
            var res = new Dictionary<string, MarkingPrototype>();

            foreach (var (key, marking) in MarkingsByCategory(category))
            {
                // Goobstation - species without hair or other markings
                if (!markingPoints.Points.ContainsKey(category))
                    continue;

                if ((markingPoints.OnlyWhitelisted || markingPoints.Points[category].OnlyWhitelisted) && marking.SpeciesRestrictions == null)
                {
                    continue;
                }

                if (marking.SpeciesRestrictions != null && !marking.SpeciesRestrictions.Contains(species))
                {
                    continue;
                }

                // Pirate: playerRestriction filter
                if (ckey != null && marking.PlayerRestrictions != null && marking.PlayerRestrictions.Count > 0 && !marking.PlayerRestrictions.Contains(ckey))
                    continue;

                res.Add(key, marking);
            }



            return res;
        }

        /// <summary>
        ///     Markings by category and sex.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="sex"></param>
        /// <remarks>
        ///     This is done per category, as enumerating over every single marking by species isn't useful.
        ///     Please make a pull request if you find a use case for that behavior.
        /// </remarks>
        /// <returns></returns>
        public IReadOnlyDictionary<string, MarkingPrototype> MarkingsByCategoryAndSex(MarkingCategories category,
            Sex sex, string? ckey = null) // Pirate ckey for restricted players
        {
            var res = new Dictionary<string, MarkingPrototype>();

            foreach (var (key, marking) in MarkingsByCategory(category))
            {
                if (marking.SexRestriction != null && marking.SexRestriction != sex)
                {
                    continue;
                }
                // Pirate: playerRestriction filter
                if (ckey != null && marking.PlayerRestrictions != null && marking.PlayerRestrictions.Count > 0 && !marking.PlayerRestrictions.Contains(ckey))
                    continue;
                res.Add(key, marking);
            }

            return res;
        }

        /// <summary>
        ///     Markings by category, species and sex.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="species"></param>
        /// <param name="sex"></param>
        /// <remarks>
        ///     This is done per category, as enumerating over every single marking by species isn't useful.
        ///     Please make a pull request if you find a use case for that behavior.
        /// </remarks>
        /// <returns></returns>
        public IReadOnlyDictionary<string, MarkingPrototype> MarkingsByCategoryAndSpeciesAndSex(MarkingCategories category,
            string species, Sex sex, string? ckey = null) // Pirate ckey for restricted players
        {
            var speciesProto = _prototypeManager.Index<SpeciesPrototype>(species);
            var onlyWhitelisted = _prototypeManager.Index(speciesProto.MarkingPoints).OnlyWhitelisted;
            var res = new Dictionary<string, MarkingPrototype>();

            foreach (var (key, marking) in MarkingsByCategory(category))
            {
                if (onlyWhitelisted && marking.SpeciesRestrictions == null)
                {
                    continue;
                }

                if (marking.SpeciesRestrictions != null && !marking.SpeciesRestrictions.Contains(species))
                {
                    continue;
                }

                if (marking.SexRestriction != null && marking.SexRestriction != sex)
                {
                    continue;
                }

                // Pirate: playerRestriction filter
                if (ckey != null && marking.PlayerRestrictions != null && marking.PlayerRestrictions.Count > 0 && !marking.PlayerRestrictions.Contains(ckey))
                    continue;

                res.Add(key, marking);
            }

            return res;
        }

        public bool TryGetMarking(Marking marking, [NotNullWhen(true)] out MarkingPrototype? markingResult)
        {
            return Markings.TryGetValue(marking.MarkingId, out markingResult);
        }

        /// <summary>
        ///     Check if a marking is valid according to the category, species, and current data this marking has.
        /// </summary>
        /// <param name="marking"></param>
        /// <param name="category"></param>
        /// <param name="species"></param>
        /// <param name="sex"></param>
        /// <returns></returns>
        public bool IsValidMarking(Marking marking, MarkingCategories category, string species, Sex sex, string? ckey = null) // Pirate ckey for restricted players
        {
            if (!TryGetMarking(marking, out var proto))
            {
                return false;
            }

            if (proto.MarkingCategory != category ||
                proto.SpeciesRestrictions != null && !proto.SpeciesRestrictions.Contains(species) ||
                proto.SexRestriction != null && proto.SexRestriction != sex)
            {
                return false;
            }

            // Pirate: playerRestriction check (ckey=null by default, for backward compatibility)
            if (ckey != null && proto.PlayerRestrictions != null && proto.PlayerRestrictions.Count > 0 && !proto.PlayerRestrictions.Contains(ckey))
            {
                return false;
            }

            if (marking.MarkingColors.Count != proto.Sprites.Count)
            {
                return false;
            }

            return true;
        }


        private void OnPrototypeReload(PrototypesReloadedEventArgs args)
        {
            if (args.WasModified<MarkingPrototype>())
                CachePrototypes();
        }

        public bool CanBeApplied(string species, Sex sex, Marking marking, IPrototypeManager? prototypeManager = null, string? ckey = null) // Pirate ckey for restricted players
        {
            IoCManager.Resolve(ref prototypeManager);

            var speciesProto = prototypeManager.Index<SpeciesPrototype>(species);
            var onlyWhitelisted = prototypeManager.Index(speciesProto.MarkingPoints).OnlyWhitelisted;

            if (!TryGetMarking(marking, out var prototype))
            {
                return false;
            }

            if (onlyWhitelisted && prototype.SpeciesRestrictions == null)
            {
                return false;
            }

            if (prototype.SpeciesRestrictions != null
                && !prototype.SpeciesRestrictions.Contains(species))
            {
                return false;
            }

            if (prototype.SexRestriction != null && prototype.SexRestriction != sex)
            {
                return false;
            }

            // Pirate: playerRestriction check (ckey=null by default)
            if (ckey != null && prototype.PlayerRestrictions != null && prototype.PlayerRestrictions.Count > 0 && !prototype.PlayerRestrictions.Contains(ckey))
            {
                return false;
            }

            return true;
        }

        // Pirate Changes start ckey for restricted players
        public bool CanBeApplied(string species, Sex sex, Marking marking, string? ckey, IPrototypeManager? prototypeManager = null)
        {
            if (!TryGetMarking(marking, out var prototype))
                return false;
            return CanBeApplied(species, sex, marking, prototypeManager) && (ckey == null || prototype.PlayerRestrictions == null || prototype.PlayerRestrictions.Count == 0 || prototype.PlayerRestrictions.Contains(ckey));
        }

        public bool CanBeApplied(string species, Sex sex, MarkingPrototype prototype, IPrototypeManager? prototypeManager = null, string? ckey = null) //Pirate changes end ckey for restricted players
        {
            IoCManager.Resolve(ref prototypeManager);

            var speciesProto = prototypeManager.Index<SpeciesPrototype>(species);
            var onlyWhitelisted = prototypeManager.Index(speciesProto.MarkingPoints).OnlyWhitelisted;

            if (onlyWhitelisted && prototype.SpeciesRestrictions == null)
            {
                return false;
            }

            if (prototype.SpeciesRestrictions != null &&
                !prototype.SpeciesRestrictions.Contains(species))
            {
                return false;
            }

            if (prototype.SexRestriction != null && prototype.SexRestriction != sex)
            {
                return false;
            }

            // Pirate: playerRestriction check (ckey=null by default)
            if (ckey != null && prototype.PlayerRestrictions != null && prototype.PlayerRestrictions.Count > 0 && !prototype.PlayerRestrictions.Contains(ckey))
            {
                return false;
            }

            return true;
        }

        public bool MustMatchSkin(string species, HumanoidVisualLayers layer, out float alpha, IPrototypeManager? prototypeManager = null)
        {
            IoCManager.Resolve(ref prototypeManager);
            var speciesProto = prototypeManager.Index<SpeciesPrototype>(species);
            if (
                !prototypeManager.Resolve(speciesProto.SpriteSet, out var baseSprites) ||
                !baseSprites.Sprites.TryGetValue(layer, out var spriteName) ||
                !prototypeManager.Resolve(spriteName, out HumanoidSpeciesSpriteLayer? sprite) ||
                sprite == null ||
                !sprite.MarkingsMatchSkin
            )
            {
                alpha = 1f;
                return false;
            }

            alpha = sprite.LayerAlpha;
            return true;
        }
    }
}