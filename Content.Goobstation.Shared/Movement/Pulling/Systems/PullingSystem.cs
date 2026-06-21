using Content.Goobstation.Common.Grab;
using Content.Goobstation.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Systems;

private void OnRefreshMovespeed(EntityUid uid, PullerComponent component, RefreshMovementSpeedModifiersEvent args)
{
    // <Trauma>
    // skip this if ApplySpeedModifier is false
    if (!component.ApplySpeedModifier)
        return;

    var speed = component.GrabStage switch
    {
        GrabStage.Soft => component.SoftGrabSpeedModifier,
