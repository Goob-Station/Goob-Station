using Content.Shared.ActionBlocker;
using Content.Shared.Alert;
using Content.Shared.Examine;
using Content.Shared.Materials;
using Content.Shared.Movement.Events;

namespace Content.Shared._CorvaxGoob.Photo;

public abstract partial class SharedPhotoSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedMaterialStorageSystem _material = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhotoCameraComponent, ExaminedEvent>(OnCameraExamined);

        SubscribeLocalEvent<PhotoCameraUserComponent, UpdateCanMoveEvent>(HandleMovementBlock);
        SubscribeLocalEvent<PhotoCameraUserComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<PhotoCameraUserComponent, ComponentShutdown>(OnShutdown);
    }

    protected virtual void OnShutdown(EntityUid uid, PhotoCameraUserComponent component, ComponentShutdown args)
    {
        _actionBlockerSystem.UpdateCanMove(uid);
        _alerts.ClearAlert(uid, component.AlertPrototype);
    }

    private void OnStartup(EntityUid uid, PhotoCameraUserComponent component, ComponentStartup args)
    {
        _actionBlockerSystem.UpdateCanMove(uid);
        _alerts.ShowAlert(uid, component.AlertPrototype);
    }

    private void HandleMovementBlock(EntityUid uid, PhotoCameraUserComponent component, UpdateCanMoveEvent args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        args.Cancel();
    }

    private void OnCameraExamined(EntityUid uid, PhotoCameraComponent component, ExaminedEvent args)
    {
        int paperLeft = (int)MathF.Ceiling(_material.GetMaterialAmount(uid, component.CardMaterial) / component.CardCost);
        string message = Loc.GetString("photo-camera-examined-paper-left", ("count", paperLeft));

        args.PushMarkup(message);
    }
}
