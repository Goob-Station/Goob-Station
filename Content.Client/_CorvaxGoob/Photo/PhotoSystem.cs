using Content.Client._CorvaxGoob.Photo.UI;
using Content.Shared._CorvaxGoob.Photo;

namespace Content.Client._CorvaxGoob.Photo;

public sealed partial class PhotoSystem : SharedPhotoSystem
{
    public Dictionary<PhotoCameraComponent, PhotoCameraBoundUserInterface> ActiveCameras = new();

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var (component, window) in ActiveCameras)
        {
            window.UpdateControl(component, frameTime);
        }
    }

    public void OpenCameraUi(PhotoCameraComponent component, PhotoCameraBoundUserInterface window)
    {
        if (ActiveCameras.ContainsKey(component))
            return;

        ActiveCameras.Add(component, window);
    }

    public void CloseCameraUi(PhotoCameraComponent component)
    {
        if (!ActiveCameras.ContainsKey(component))
            return;

        ActiveCameras.Remove(component);
    }
}
