using Content.Shared.Chat;
using Content.Shared.Light;
using Content.Shared.Interaction;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Pinpointer;
using Robust.Shared.Map;
using SixLabors.ImageSharp.Processing.Processors.Filters;

namespace Content.Goobstation.Shared.JanitorCallButton;

public sealed partial class JanitorCallButtonSystem : EntitySystem
{
    [Dependency] private SlimPoweredLightSystem _light = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedChatSystem _chat = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    /*
    [Dependency] private SharedNavMapSystem _navMap = default!;
    [Dependency] private SharedNavMapSystem.NavMapBeacon _navBeacon = default!;
    */

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<JanitorCallButtonComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<JanitorCallButtonComponent, InteractHandEvent>(OnInteractHand);
    }

    private void OnMapInit(Entity<JanitorCallButtonComponent> ent, ref MapInitEvent args)
    {
        if (TryComp<SlimPoweredLightComponent>(ent.Owner, out var light))
            UpdateVisuals(ent.Owner, light.Enabled);
    }

    private void OnInteractHand(Entity<JanitorCallButtonComponent> ent, ref InteractHandEvent args)
    {
        var comp = Comp<JanitorCallButtonComponent>(ent.Owner);

        if (TryComp<JanitorCallButtonComponent>(ent.Owner, out var button))
            comp.enabled = !comp.enabled;

        if (args.Handled || !TryComp<SlimPoweredLightComponent>(ent.Owner, out var light))
            return;

        var enabled = !light.Enabled;
        _light.SetEnabled((ent.Owner, light), enabled);
        UpdateVisuals(ent.Owner, enabled);
        RaiseLocalEvent(ent.Owner, new JanitorCallButtonPressedEvent());
        args.Handled = true;
    }

    private void CallJanitor(Entity<JanitorCallButtonComponent> ent)
    {
        var stationBeacon = GetNearestBeacon(ent);

        _chat.TrySendInGameICMessage(
            ent.Owner,
            $":s Called janitor near {stationBeacon}",
            InGameICChatType.Speak,
            ChatTransmitRange.Normal
            );
    }

    private string GetNearestBeacon(Entity<JanitorCallButtonComponent> ent)
    {
        var coordinates = _transform.GetMapCoordinates(
            ent.Owner,
            Transform(ent.Owner)
        );

        var nearestName = "неизвестное место";
        var minDistance = float.PositiveInfinity;

        var query = EntityQueryEnumerator<
            ConfigurableNavMapBeaconComponent,
            NavMapBeaconComponent,
            TransformComponent>();

        while (query.MoveNext(
                   out var uid,
                   out _,
                   out var beacon,
                   out var beaconTransform))
        {
            if (!beacon.Enabled || string.IsNullOrWhiteSpace(beacon.Text))
                continue;

            if (coordinates.MapId != beaconTransform.MapID)
                continue;

            var beaconPosition = _transform.GetWorldPosition(beaconTransform);

            var distance = (coordinates.Position - beaconPosition).LengthSquared();

            if (distance >= minDistance)
                continue;

            minDistance = distance;
            nearestName = beacon.Text;
        }

        return nearestName;
    }

    private void UpdateVisuals(EntityUid uid, bool enabled)
    {
        _appearance.SetData(uid, PoweredLightVisuals.BulbState, enabled ? PoweredLightState.On : PoweredLightState.Off);
    }
}
