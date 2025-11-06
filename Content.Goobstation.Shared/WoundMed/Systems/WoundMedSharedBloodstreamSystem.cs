using Content.Shared.Chemistry.EntitySystems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Body._Goobstation;
using Content.Shared.Body.Systems;

namespace Content.Goobstation.Shared.WoundMed.Systems;

public abstract class WoundMedSharedBloodstreamSystem : EntitySystem
{
    [Dependency] protected readonly SharedSolutionContainerSystem SolutionContainer = default!;
    [Dependency] private readonly ConsciousnessSystem _consciousness = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly WoundSystem _wound = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UpdateBloodstreamEvent>(OnUpdateBloodstream);
    }

    /// <summary>
    /// Handles blood loss and bleeding effects for an entity.
    /// - Calculates total bleeding from all wounds
    /// - Updates woundable components with current bleed rates
    /// - Calculates blood loss effects
    /// - Updates consciousness based on blood loss
    /// - Returns updated bleed amount via the event
    /// </summary>
    /// <param name="args">Event containing entity, bloodstream, and bleed amount data</param>
    private void OnUpdateBloodstream(ref UpdateBloodstreamEvent args)
    {
        // Extract event data
        var uid = args.Entity; // Entity UID being processed
        var bloodstream = args.Bloodstream; // Reference to the bloodstream component
        var bleedAmount = args.BleedAmount; // Current bleed amount from the event

        // Check if entity has a nerve system component
        if (!_consciousness.TryGetNerveSystem(uid, out var nerveSys) || nerveSys is not { } nerveComponent)
            return;

        // Get the blood solution from the bloodstream
        if (args.Bloodstream.BloodSolution?.Comp.Solution is not { } solution)
            return;

        var total = FixedPoint2.Zero;

        // Iterate through all body parts
        foreach (var (bodyPart, _) in _body.GetBodyChildren(uid))
        {
            var totalPartBleeds = FixedPoint2.Zero;

            // Check each wound on the body part
            foreach (var (wound, _) in _wound.GetWoundableWounds(bodyPart))
            {
                if (!TryComp<BleedInflicterComponent>(wound, out var bleeds))
                    continue; // Skip if wound doesn't cause bleeding

                total += bleeds.BleedingAmount; // Add to total bleeding
                totalPartBleeds = bleeds.BleedingAmount; // Set current part's bleeding
            }

            // Update woundable component with current part's bleed amount
            if (TryComp<WoundableComponent>(bodyPart, out var woundable))
            {
                woundable.Bleeds = totalPartBleeds;
            }
        }

        // Calculate missing blood volume
        var missingBlood = bloodstream.BloodMaxVolume - solution.Volume;

        // Calculate new bleed amount based on total wounds
        bleedAmount = (float) total / 4; // This updates the local variable only
        args.BleedAmount = bleedAmount; // Update the event with new bleed amount

        // Apply consciousness modifier based on blood loss
        if (!_consciousness.SetConsciousnessModifier(
                uid,
                nerveSys.Value, // Using the nerve system component
                -missingBlood / 4, // Negative value for damage
                identifier: "Bleeding",
                type: ConsciousnessModType.Pain))
        {
            // If setting modifier fails, try adding a new one
            _consciousness.AddConsciousnessModifier(
                uid,
                nerveSys.Value,
                -missingBlood / 4,
                identifier: "Bleeding",
                type: ConsciousnessModType.Pain);
        }
    }
}


