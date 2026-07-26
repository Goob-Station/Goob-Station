// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Humanoid;
using Content.Server.Preferences.Managers;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;
using System.Numerics; // Goobstation

namespace Content.Server.GameTicking.Rules;

public sealed class AntagLoadProfileRuleSystem : GameRuleSystem<AntagLoadProfileRuleComponent>
{
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _sharedHumanoid = default!; // Goobstation

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AntagLoadProfileRuleComponent, AntagSelectEntityEvent>(OnSelectEntity);
    }

    private void OnSelectEntity(Entity<AntagLoadProfileRuleComponent> ent, ref AntagSelectEntityEvent args)
    {
        if (args.Handled)
            return;

        var profile = args.Session != null
            ? _prefs.GetPreferences(args.Session.UserId).SelectedCharacter as HumanoidCharacterProfile
            : HumanoidCharacterProfile.RandomWithSpecies();


        if (profile?.Species is not { } speciesId || !_proto.Resolve(speciesId, out var species))
        {
            species = _proto.Index<SpeciesPrototype>(SharedHumanoidAppearanceSystem.DefaultSpecies);
        }

        if (ent.Comp.SpeciesOverride != null
            && (ent.Comp.AlwaysUseSpeciesOverride || ( ent.Comp.SpeciesOverrideBlacklist?.Contains(new ProtoId<SpeciesPrototype>(species.ID)) ?? false))) // Goob edit
        {
            species = _proto.Index(ent.Comp.SpeciesOverride.Value);
        }

        if (ent.Comp.SpeciesHardOverride is not null) // Shitmed - Starlight Abductors
            species = _proto.Index(ent.Comp.SpeciesHardOverride.Value); // Shitmed - Starlight Abductors

        args.Entity = Spawn(species.Prototype);
        _humanoid.LoadProfile(args.Entity.Value, profile?.WithSpecies(species.ID));

        // Goobstation start - Make entities spawn at max size for their species
        if (ent.Comp.ForceMaxSize
            && TryComp<HumanoidAppearanceComponent>(args.Entity.Value, out var humanoid))
        {
            var maxScale = new Vector2(species.MaxWidth, species.MaxHeight);
            _sharedHumanoid.SetScale(args.Entity.Value, maxScale, true, humanoid);
        }
        // Goobstation end
    }
}
