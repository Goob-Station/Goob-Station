using Content.Pirate.Shared.ReadyManifest;

namespace Content.Pirate.Client.ReadyManifest;

public sealed class ReadyManifestSystem : EntitySystem
{

    public void RequestReadyManifest()
    {
        RaiseNetworkEvent(new RequestReadyManifestMessage());
    }
}
