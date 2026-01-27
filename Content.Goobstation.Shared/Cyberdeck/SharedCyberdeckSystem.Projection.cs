using Content.Goobstation.Common.Cyberdeck.Components;
using Content.Goobstation.Common.Interaction;
using Content.Shared.Movement.Components;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.Cyberdeck;

// WARNING: if you want to reuse this code, it's better to make it a general system first instead of copy-pasting
// Abductors also have their own version of this btw
public abstract partial class SharedCyberdeckSystem
{
    private void InitializeProjection()
    {
        SubscribeLocalEvent<CyberdeckUserComponent, CyberdeckVisionEvent>(OnCyberVisionUsed);
        SubscribeLocalEvent<CyberdeckUserComponent, CyberdeckVisionReturnEvent>(OnCyberVisionReturn);
    }

    /// <summary>
    /// Attaches a player to projection if it already exists,
    /// otherwise creates it and does the same but on server side.
    /// </summary>
    /// <param name="user"></param>
    private void AttachToProjection(Entity<CyberdeckUserComponent> user)
    {
        if (user.Comp.InProjection)
            return;

        // At first, we just add visuals & actions, because they're easily predicted
        EnsureComp<StationAiOverlayComponent>(user.Owner);
        EnsureComp<CyberdeckOverlayComponent>(user.Owner);
        EnsureComp<NoNormalInteractionComponent>(user.Owner);

        _actions.AddAction(user.Owner, ref user.Comp.ReturnAction, user.Comp.ReturnActionId);
        _actions.RemoveAction(user.Owner, user.Comp.VisionAction);
        user.Comp.VisionAction = null; // Shitcode to prevent errors

        _audio.PlayLocal(user.Comp.DiveStartSound, user.Owner, user.Owner);
        _bossMusic.StartBossMusic(user.Comp.DiveMusicId, user.Owner); // Ambient loop

        // Now everything becomes tricky.
        // To make everything work smoothly enough, we need to store the projection entity somewhere.
        // That means that there are 2 possible scenarios:
        // 1. Projection entity is already stored on a paused map, and we know that it exist
        // 2. Projection entity doesn't exist

        // Only in the first case we can actually do something.
        // There's also a possibility that this code just doesn't work, but honestly I have no idea.
        // I think this is because of PVS, but im not using dangerous PVS overrides and everything works already,
        // so i guess it's better to just not touch this code until we have functioning PredictedSpawn.

        // Handle second case (projection doesn't exist)
        if (user.Comp.ProjectionEntity == null
            || TerminatingOrDeleted(user.Comp.ProjectionEntity))
        {
            if (_net.IsClient)
                return;

            var newProjection = Spawn(user.Comp.ProjectionEntityId, MapCoordinates.Nullspace);
            var projectionComp = EnsureComp<CyberdeckProjectionComponent>(newProjection);

            projectionComp.RemoteEntity = user.Owner;
            user.Comp.ProjectionEntity = newProjection;

            Dirty(user.Owner, user.Comp);
            Dirty(newProjection, projectionComp);
        }

        // Handle the standard case, when we just need to pull an existing entity from Nullspace
        var projection = user.Comp.ProjectionEntity.Value;
        var position = Transform(user).Coordinates;

        Xform.SetCoordinates(projection, position);

        if (TryComp(user, out EyeComponent? eyeComp))
        {
            _eye.SetDrawFov(user, false, eyeComp);
            _eye.SetTarget(user, projection, eyeComp);
        }

        _mover.SetRelay(user, projection);
        user.Comp.InProjection = true;
    }

    /// <summary>
    /// Detaches player from a projection forcefully, and sends an existing projection to Nullspace.
    /// </summary>
    private void DetachFromProjection(Entity<CyberdeckUserComponent> user)
    {
        if (user.Comp.ProjectionEntity == null
            || !user.Comp.InProjection)
            return;

        RemComp<StationAiOverlayComponent>(user);
        RemComp<CyberdeckOverlayComponent>(user);
        RemComp<NoNormalInteractionComponent>(user);

        _actions.AddAction(user, ref user.Comp.VisionAction, user.Comp.VisionActionId);
        _actions.RemoveAction(user.Owner, user.Comp.ReturnAction);
        user.Comp.ReturnAction = null; // Shitcode to prevent errors

        _audio.PlayLocal(user.Comp.DiveExitSound, user.Owner, user.Owner);
        _bossMusic.EndAllMusic(user.Owner);

        if (TryComp(user, out EyeComponent? eyeComp))
        {
            _eye.SetDrawFov(user, true, eyeComp);
            _eye.SetTarget(user, null, eyeComp);
        }

        RemComp<RelayInputMoverComponent>(user);

        // We did everything to put the player back in place,
        // now let's try to save the projection for smoother prediction
        user.Comp.InProjection = false;

        if (TerminatingOrDeleted(user.Comp.ProjectionEntity))
            return;

        var projection = user.Comp.ProjectionEntity.Value;

        // This probably is a dirty solution, but surprisingly you can't send entities to Nullspace...
        // So I'll just steal an already existing paused map instead of shitspamming with a new one.
        // TODO: Make a universal paused map to store things on
        _cryostorage.EnsurePausedMap();
        if (_cryostorage.PausedMap == null)
        {
            Log.Error("CryoSleep map was unexpectedly null");
            return;
        }

        Xform.SetParent(projection, _cryostorage.PausedMap.Value);
        Dirty(user.Owner, user.Comp);
    }

    private void OnCyberVisionUsed(Entity<CyberdeckUserComponent> ent, ref CyberdeckVisionEvent args)
    {
        if (args.Handled)
            return;

        var (uid, comp) = ent;

        if (!UseCharges(uid, comp.CyberVisionAbilityCost))
            return;

        AttachToProjection(ent);
        args.Handled = true;
    }

    private void OnCyberVisionReturn(Entity<CyberdeckUserComponent> ent, ref CyberdeckVisionReturnEvent args)
    {
        if (args.Handled)
            return;

        DetachFromProjection(ent);
        args.Handled = true;
    }
}
