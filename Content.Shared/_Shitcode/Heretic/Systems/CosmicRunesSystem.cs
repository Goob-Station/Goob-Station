using System.Linq;
using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Coordinates;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Shared._Shitcode.Heretic.Systems;

public sealed class CosmicRunesSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticCosmicRuneComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<HereticCosmicRuneComponent, ActivateInWorldEvent>(OnActivate);
    }

    private void OnActivate(Entity<HereticCosmicRuneComponent> ent, ref ActivateInWorldEvent args)
    {
        if (Teleport(ent, args.User))
            args.Handled = true;
    }

    private void OnInteract(Entity<HereticCosmicRuneComponent> ent, ref InteractHandEvent args)
    {
        if (Teleport(ent, args.User))
            args.Handled = true;
    }

    private bool Teleport(Entity<HereticCosmicRuneComponent> ent, EntityUid user)
    {
        if (HasComp<FadingTimedDespawnComponent>(ent))
            return false;

        if (!Exists(ent.Comp.LinkedRune) || !TryComp(ent.Comp.LinkedRune.Value, out TransformComponent? xform) ||
            !xform.Coordinates.IsValid(EntityManager) || HasComp<FadingTimedDespawnComponent>(ent.Comp.LinkedRune.Value))
        {
            if (_net.IsServer) // Client can have rune deleted due to PVS but can exist on server
                _popup.PopupEntity(Loc.GetString("heretic-cosmic-rune-fail-unlinked"), user, user);
            return false;
        }

        if (HasComp<StarMarkComponent>(user))
        {
            _popup.PopupPredicted(Loc.GetString("heretic-cosmic-rune-fail-star-mark"), user, user);
            return false;
        }

        if (!_transform.InRange(ent.Owner, user, ent.Comp.Range))
        {
            _popup.PopupPredicted(Loc.GetString("heretic-cosmic-rune-fail-range"), user, user);
            return false;
        }

        if (_net.IsServer)
        {
            _audio.PlayPvs(ent.Comp.Sound, ent);
            _audio.PlayPvs(ent.Comp.Sound, ent.Comp.LinkedRune.Value);
            SpawnAttachedTo(ent.Comp.Effect, ent.Owner.ToCoordinates());
            SpawnAttachedTo(ent.Comp.Effect, ent.Comp.LinkedRune.Value.ToCoordinates());
        }

        var toTeleport = _lookup.GetEntitiesInRange(Transform(ent).Coordinates, ent.Comp.Range, LookupFlags.Dynamic)
            .Where(HasComp<StarMarkComponent>)
            .ToHashSet();
        toTeleport.Add(user);

        foreach (var entity in toTeleport)
        {
            _pulling.StopAllPulls(entity);
            _transform.SetCoordinates(entity, xform.Coordinates);
        }

        return true;
    }
}
