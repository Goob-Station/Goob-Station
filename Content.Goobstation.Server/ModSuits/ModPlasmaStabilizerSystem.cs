using Content.Goobstation.Shared.ModSuits;
using Content.Goobstation.Shared.ModSuits.Modules;
using Content.Server._EinsteinEngines.Atmos.Components;
using Content.Server._EinsteinEngines.Atmos.EntitySystems;
using Content.Shared._Shitmed.Targeting;

namespace Content.Goobstation.Server.ModSuits;

public sealed partial class ModPlasmaStabilizerSystem : EntitySystem
{
    [Dependency] private IgniteFromGasSystem _ignite = default!;

    [SubscribeLocalEvent]
    private void OnActivated(Entity<ModPlasmaStabilizerModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        if (HasComp<IgniteFromGasImmunityComponent>(args.Suit))
            return;

        var immunity = AddComp<IgniteFromGasImmunityComponent>(args.Suit);
        immunity.Parts = new HashSet<TargetBodyPart> { TargetBodyPart.Head };

        if (args.Wearer is { } wearer)
            _ignite.UpdateIgniteImmunity(wearer);
    }

    [SubscribeLocalEvent]
    private void OnDeactivated(Entity<ModPlasmaStabilizerModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        if (TerminatingOrDeleted(args.Suit) || !RemComp<IgniteFromGasImmunityComponent>(args.Suit))
            return;

        if (args.Wearer is { } wearer && !TerminatingOrDeleted(wearer))
            _ignite.UpdateIgniteImmunity(wearer);
    }
}
