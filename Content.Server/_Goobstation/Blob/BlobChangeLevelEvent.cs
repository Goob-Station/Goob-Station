using Content.Server.GameTicking.Rules.Components;
using Content.Shared._Goobstation.Blob.Components;

namespace Content.Server._Goobstation.Blob;

public sealed class BlobChangeLevelEvent : EntityEventArgs
{
    public EntityUid Station;
    public BlobStage Level;
}
