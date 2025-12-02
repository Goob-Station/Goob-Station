using Content.Server.Hands.Systems;
using Content.Server.Materials;
using Content.Server.Popups;
using Content.Shared._CorvaxGoob.Photo;
using Content.Shared.Materials;
using Content.Shared.Timing;
using Content.Shared.UserInterface;
using Robust.Server.Audio;
using Robust.Server.GameObjects;

namespace Content.Server._CorvaxGoob.Photo;

public sealed partial class PhotoSystem : SharedPhotoSystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MaterialStorageSystem _material = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    //96 KB
    const int MAX_SIZE = 1024 * 96;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhotoCameraComponent, AfterActivatableUIOpenEvent>(OnOpenCameraInterface);
        Subs.BuiEvents<PhotoCameraComponent>(PhotoCameraUiKey.Key, subs =>
        {
            subs.Event<BoundUIClosedEvent>(OnCameraBoundUiClose);
            subs.Event<PhotoCameraTakeImageMessage>(OnTakeImageMessage);
        });
        SubscribeLocalEvent<PhotoCameraComponent, MaterialAmountChangedEvent>(OnPaperInserted);

        SubscribeLocalEvent<PhotoCardComponent, AfterActivatableUIOpenEvent>(OnOpenCardInterface);
    }

    private void OnOpenCameraInterface(EntityUid uid, PhotoCameraComponent component, AfterActivatableUIOpenEvent args)
    {
        UpdateCameraInterface(uid, component);

        component.User = args.User;
        EnsureComp<PhotoCameraUserComponent>(args.User);
    }

    private void OnCameraBoundUiClose(EntityUid uid, PhotoCameraComponent component, BoundUIClosedEvent args)
    {
        if (component.User == null)
            return;

        RemComp<PhotoCameraUserComponent>(component.User.Value);
        component.User = null;
    }

    private void OnTakeImageMessage(EntityUid uid, PhotoCameraComponent component, PhotoCameraTakeImageMessage message)
    {
        if (message.Data.Length > MAX_SIZE)
            return;

        if (!CheckPngSignature(message.Data))
            return;

        if (TryTakeImage(uid, component, message.Data))
            RaiseLocalEvent(new PhotoCameraTakeImageEvent(uid, message.Actor, message.PhotoPosition, message.Zoom));
    }

    private void UpdateCameraInterface(EntityUid uid, PhotoCameraComponent component, EntityUid? player = null)
    {
        bool hasPaper = _material.CanChangeMaterialAmount(uid, component.CardMaterial, -component.CardCost);

        var state = new PhotoCameraUiState(GetNetEntity(uid), hasPaper);
        _userInterface.SetUiState(uid, PhotoCameraUiKey.Key, state);
    }

    private void OnPaperInserted(EntityUid uid, PhotoCameraComponent component, MaterialAmountChangedEvent args)
    {
        if (TryComp<MaterialStorageComponent>(uid, out var storage))
            Dirty(uid, storage);

        UpdateCameraInterface(uid, component, component.User);
    }

    private bool TryTakeImage(EntityUid uid, PhotoCameraComponent component, byte[] imageData)
    {
        if (_delay.IsDelayed(uid))
            return false;

        _delay.TryResetDelay(uid);

        var printCard = PrintCard(uid, component, imageData);

        if (printCard)
            _audio.PlayPvs(component.PhotoSound, uid);
        else
            _audio.PlayPvs(component.ErrorSound, uid);

        return printCard;
    }

    private bool PrintCard(EntityUid uid, PhotoCameraComponent component, byte[] imageData)
    {
        if (!_material.TryChangeMaterialAmount(uid, component.CardMaterial, -component.CardCost))
        {
            if (component.User != null)
                _popup.PopupEntity(Loc.GetString("photo-camera-no-paper"), uid, component.User.Value);

            return false;
        }

        var card = Spawn(component.CardPrototype);
        _transform.SetMapCoordinates(card, _transform.GetMapCoordinates(uid));

        if (TryComp<PhotoCardComponent>(card, out var photo))
            photo.ImageData = imageData;

        if (component.User != null)
            _hands.TryPickupAnyHand(component.User.Value, card);

        UpdateCameraInterface(uid, component, component.User);

        return true;
    }

    private static bool CheckPngSignature(ReadOnlySpan<byte> data)
    {
        if (data.Length < 8) return false;
        return data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47 &&
                data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A;
    }

    // Photo Card

    private void OnOpenCardInterface(EntityUid uid, PhotoCardComponent component, AfterActivatableUIOpenEvent args)
    {
        var state = new PhotoCardUiState(component.ImageData);
        _userInterface.SetUiState(uid, PhotoCardUiKey.Key, state);
    }
}
