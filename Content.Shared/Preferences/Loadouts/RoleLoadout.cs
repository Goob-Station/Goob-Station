// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.CCVar;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Random;
using Robust.Shared.Collections;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences.Loadouts;

/// <summary>
/// Contains all of the selected data for a role's loadout.
/// </summary>
[Serializable, NetSerializable, DataDefinition]
public sealed partial class RoleLoadout : IEquatable<RoleLoadout>
{
    [DataField]
    public ProtoId<RoleLoadoutPrototype> Role;

    [DataField]
    public Dictionary<ProtoId<LoadoutGroupPrototype>, List<Loadout>> SelectedLoadouts = new();

    /// <summary>
    /// Loadout specific name.
    /// </summary>
    [DataField]
    public string? EntityName;

    /*
     * Loadout-specific data used for validation.
     */

    public int? Points;

    public RoleLoadout(ProtoId<RoleLoadoutPrototype> role)
    {
        Role = role;
    }

    public RoleLoadout Clone()
    {
        var weh = new RoleLoadout(Role);

        foreach (var selected in SelectedLoadouts)
        {
            weh.SelectedLoadouts.Add(selected.Key, selected.Value.Select(loadout => loadout.Clone()).ToList()); // Pirate: loadout
        }

        weh.EntityName = EntityName;
        weh.Points = Points; // Pirate: loadout

        return weh;
    }

    /// <summary>
    /// Ensures all prototypes exist and effects can be applied.
    /// </summary>
    public void EnsureValid(HumanoidCharacterProfile profile, ICommonSession session, IDependencyCollection collection)
    {
        var groupRemove = new ValueList<string>();
        var protoManager = collection.Resolve<IPrototypeManager>();
        var configManager = collection.Resolve<IConfigurationManager>();

        if (!protoManager.TryIndex(Role, out var roleProto))
        {
            EntityName = null;
            SelectedLoadouts.Clear();
            return;
        }

        // Remove name not allowed.
        if (!roleProto.CanCustomizeName)
        {
            EntityName = null;
        }

        // Validate name length
        // TODO: Probably allow regex to be supplied?
        if (EntityName != null)
        {
            var name = EntityName.Trim();
            var maxNameLength = configManager.GetCVar(CCVars.MaxNameLength);

            if (name.Length > maxNameLength)
            {
                EntityName = name[..maxNameLength];
            }

            if (name.Length == 0)
            {
                EntityName = null;
            }
        }

        var explicitGroups = SelectedLoadouts.Keys.ToHashSet(); // Pirate: loadout

        // In some instances we might not have picked up a new group for existing data.
        foreach (var groupProto in roleProto.Groups.Concat(GlobalLoadoutGroups.Groups)) // Pirate: loadout
        {
            if (SelectedLoadouts.ContainsKey(groupProto))
                continue;

            // Data will get set below.
            SelectedLoadouts[groupProto] = new List<Loadout>();
        }

        // Reset points to recalculate.
        Points = roleProto.Points;

        foreach (var (group, groupLoadouts) in SelectedLoadouts)
        {
            // Check the group is even valid for this role.
            if (!roleProto.Groups.Contains(group) && !GlobalLoadoutGroups.Groups.Contains(group)) // Pirate: loadout
            {
                groupRemove.Add(group);
                continue;
            }

            // Dump if Group doesn't exist
            if (!protoManager.TryIndex(group, out var groupProto))
            {
                groupRemove.Add(group);
                continue;
            }

            var loadouts = groupLoadouts[..Math.Min(groupLoadouts.Count, groupProto.MaxLimit)];
            var allValidLoadoutIds = groupProto.GetAllLoadouts(protoManager).ToHashSet();   // Pirate - port frontier subgroups

            // Validate first
            for (var i = loadouts.Count - 1; i >= 0; i--)
            {
                var loadout = loadouts[i];

                // Old prototype or otherwise invalid.
                if (!protoManager.TryIndex(loadout.Prototype, out var loadoutProto))
                {
                    loadouts.RemoveAt(i);
                    continue;
                }

                // Malicious client maybe, check the group even has it.
                // if (!groupProto.Loadouts.Contains(loadout.Prototype))
                if (!allValidLoadoutIds.Contains(loadout.Prototype)) // Pirate - port frontier subgroups
                {
                    loadouts.RemoveAt(i);
                    continue;
                }

                // Validate the loadout can be applied (e.g. points).
                if (!IsValid(profile, session, loadout.Prototype, collection, out _))
                {
                    loadouts.RemoveAt(i);
                    continue;
                }

                if (!loadoutProto.CustomColorTint || string.IsNullOrEmpty(loadout.CustomColorTint) || !loadout.IsValidColorTint()) // Pirate: loadout
                    loadout.CustomColorTint = null; // Pirate: loadout

                #region Pirate: loadout
                loadout.CustomName = SanitizeCustomText(loadout.CustomName, configManager.GetCVar(CCVars.MaxNameLength));
                loadout.CustomDescription = SanitizeCustomText(loadout.CustomDescription, MaxLoadoutDescriptionLength);
                #endregion

                Apply(loadoutProto);
            }

            // Apply defaults if required
            // Technically it's possible for someone to game themselves into loadouts they shouldn't have
            // If you put invalid ones first but that's your fault for not using sensible defaults
            var pirateHasLoadoutForGroupSlot = PirateHasLoadoutForGroupSlot(group, groupProto, loadouts, profile, session, collection, protoManager); // Pirate: loadout
            var intentionallyEmptyGroup = explicitGroups.Contains(group) && groupLoadouts.Count == 0; // Pirate: loadout
            if (loadouts.Count < groupProto.MinLimit && !pirateHasLoadoutForGroupSlot && !intentionallyEmptyGroup) // Pirate: loadout
            {
                //foreach (var protoId in groupProto.Loadouts)
                foreach (var protoId in groupProto.GetAllLoadouts(protoManager).Distinct()) // Pirate - port frontier subgroups
                {
                    if (loadouts.Count >= groupProto.MinLimit)
                        break;

                    if (!protoManager.TryIndex(protoId, out var loadoutProto))
                        continue;

                    var defaultLoadout = new Loadout()
                    {
                        Prototype = loadoutProto.ID,
                    };

                    if (loadouts.Contains(defaultLoadout))
                        continue;

                    // Not valid so don't default to it anyway.
                    if (!IsValid(profile, session, defaultLoadout.Prototype, collection, out _))
                        continue;

                    loadouts.Add(defaultLoadout);
                    Apply(loadoutProto);
                }
            }

            SelectedLoadouts[group] = loadouts;
        }

        foreach (var value in groupRemove)
        {
            SelectedLoadouts.Remove(value);
        }
    }

    #region Pirate: loadout
    /// <summary>
    /// Maximum length for a loadout item's custom description.
    /// </summary>
    private const int MaxLoadoutDescriptionLength = 256;

    /// <summary>
    /// Trims a player-supplied custom name/description and clamps it to <paramref name="maxLength"/>.
    /// Returns null when the result is empty so the item falls back to its default.
    /// </summary>
    private static string? SanitizeCustomText(string? text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var trimmed = text.Trim();
        if (trimmed.Length > maxLength)
            trimmed = trimmed[..maxLength];

        return trimmed.Length == 0 ? null : trimmed;
    }

    private bool PirateHasLoadoutForGroupSlot(
        ProtoId<LoadoutGroupPrototype> currentGroup,
        LoadoutGroupPrototype currentGroupProto,
        List<Loadout> currentLoadouts,
        HumanoidCharacterProfile profile,
        ICommonSession session,
        IDependencyCollection collection,
        IPrototypeManager protoManager)
    {
        if (currentLoadouts.Count >= currentGroupProto.MinLimit)
            return false;

        var groupSlots = currentGroupProto.GetAllLoadouts(protoManager)
            .Select(protoId => protoManager.TryIndex(protoId, out LoadoutPrototype? loadoutProto)
                ? loadoutProto.Equipment.Keys
                : Enumerable.Empty<string>())
            .SelectMany(slots => slots)
            .ToHashSet();

        if (groupSlots.Count == 0)
            return false;

        var currentGroupIsGeneric = GlobalLoadoutGroups.Groups.Contains(currentGroup);

        foreach (var (group, loadouts) in SelectedLoadouts)
        {
            if (group == currentGroup || GlobalLoadoutGroups.Groups.Contains(group) != currentGroupIsGeneric)
                continue;

            foreach (var loadout in loadouts)
            {
                if (!protoManager.TryIndex(loadout.Prototype, out LoadoutPrototype? loadoutProto) ||
                    !loadoutProto.Equipment.Keys.Any(groupSlots.Contains) ||
                    !IsValid(profile, session, loadout.Prototype, collection, out _))
                    continue;

                return true;
            }
        }

        return false;
    }
    #endregion

    private void Apply(LoadoutPrototype loadoutProto)
    {
        foreach (var effect in loadoutProto.Effects)
        {
            effect.Apply(this);
        }
    }

    /// <summary>
    /// Resets the selected loadouts to default if no data is present.
    /// </summary>
    public void SetDefault(HumanoidCharacterProfile? profile, ICommonSession? session, IPrototypeManager protoManager, bool force = false)
    {
        if (profile == null)
            return;

        if (force)
            SelectedLoadouts.Clear();

        var collection = IoCManager.Instance!;
        var roleProto = protoManager.Index(Role);

        var defaultGroups = roleProto.Groups.Concat(GlobalLoadoutGroups.Groups).ToList(); // Pirate: loadout
        for (var i = defaultGroups.Count - 1; i >= 0; i--) // Pirate: loadout
        {
            var group = defaultGroups[i]; // Pirate: loadout

            if (!protoManager.TryIndex(group, out var groupProto))
                continue;

            if (SelectedLoadouts.ContainsKey(group))
                continue;

            var loadouts = new List<Loadout>();
            SelectedLoadouts[group] = loadouts;

            if (groupProto.MinLimit > 0 || loadouts.Count < groupProto.DefaultSelected)
            {
                // Apply any loadouts we can.
                //foreach (var protoId in groupProto.Loadouts)
                foreach (var protoId in groupProto.GetAllLoadouts(protoManager).Distinct()) // Pirate - port frontier subgroups
                {
                    // Reached the limit, time to stop
                    if (loadouts.Count >= Math.Max(groupProto.MinLimit, groupProto.DefaultSelected))
                        break;

                    if (!protoManager.TryIndex(protoId, out var loadoutProto))
                        continue;

                    var defaultLoadout = new Loadout()
                    {
                        Prototype = loadoutProto.ID,
                    };

                    // Not valid so don't default to it anyway.
                    if (!IsValid(profile, session, defaultLoadout.Prototype, collection, out _))
                        continue;

                    loadouts.Add(defaultLoadout);
                    Apply(loadoutProto);
                }
            }
        }
    }

    /// <summary>
    /// Returns whether a loadout is valid or not.
    /// </summary>
    public bool IsValid(HumanoidCharacterProfile profile, ICommonSession? session, ProtoId<LoadoutPrototype> loadout, IDependencyCollection collection, [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = null;

        var protoManager = collection.Resolve<IPrototypeManager>();

        if (!protoManager.TryIndex(loadout, out var loadoutProto))
        {
            // Uhh
            reason = FormattedMessage.FromMarkupOrThrow("");
            return false;
        }

        if (!protoManager.HasIndex(Role))
        {
            reason = FormattedMessage.FromUnformatted("loadouts-prototype-missing");
            return false;
        }

        var valid = true;

        foreach (var effect in loadoutProto.Effects)
        {
            valid = valid && effect.Validate(profile, this, session, collection, out reason);
        }

        return valid;
    }

    /// <summary>
    /// Applies the specified loadout to this group.
    /// </summary>
    public bool AddLoadout(ProtoId<LoadoutGroupPrototype> selectedGroup, ProtoId<LoadoutPrototype> selectedLoadout, IPrototypeManager protoManager, string? customColorTint = null, string? customName = null, string? customDescription = null) // Pirate: loadout
    {
        var groupLoadouts = SelectedLoadouts[selectedGroup];

        // Need to unselect existing ones if we're at or above limit
        var limit = Math.Max(0, groupLoadouts.Count + 1 - protoManager.Index(selectedGroup).MaxLimit);

        for (var i = 0; i < groupLoadouts.Count; i++)
        {
            var loadout = groupLoadouts[i];

            if (loadout.Prototype != selectedLoadout)
            {
                // Remove any other loadouts that might push it above the limit.
                if (limit > 0)
                {
                    limit--;
                    groupLoadouts.RemoveAt(i);
                    i--;
                }

                continue;
            }

            DebugTools.Assert(false);
            return false;
        }

        groupLoadouts.Add(new Loadout()
        {
            Prototype = selectedLoadout,
            CustomColorTint = customColorTint, // Pirate: loadout
            CustomName = customName, // Pirate: loadout
            CustomDescription = customDescription, // Pirate: loadout
        });

        return true;
    }

    /// <summary>
    /// Removed the specified loadout from this group.
    /// </summary>
    public bool RemoveLoadout(ProtoId<LoadoutGroupPrototype> selectedGroup, ProtoId<LoadoutPrototype> selectedLoadout, IPrototypeManager protoManager)
    {
        // Although this may bring us below minimum we'll let EnsureValid handle it.

        var groupLoadouts = SelectedLoadouts[selectedGroup];

        for (var i = 0; i < groupLoadouts.Count; i++)
        {
            var loadout = groupLoadouts[i];

            if (loadout.Prototype != selectedLoadout)
                continue;

            groupLoadouts.RemoveAt(i);
            return true;
        }

        return false;
    }

    public bool Equals(RoleLoadout? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (!Role.Equals(other.Role) ||
            SelectedLoadouts.Count != other.SelectedLoadouts.Count ||
            Points != other.Points ||
            EntityName != other.EntityName)
        {
            return false;
        }

        // Tried using SequenceEqual but it stinky so.
        foreach (var (key, value) in SelectedLoadouts)
        {
            if (!other.SelectedLoadouts.TryGetValue(key, out var otherValue) ||
                !otherValue.SequenceEqual(value))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is RoleLoadout other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Role, SelectedLoadouts, Points);
    }
}
