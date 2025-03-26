using Content.Goobstation.Server.Blob.GameTicking;
using Content.Server.GameTicking.Rules.Components;

namespace Content.Goobstation.Server.Blob;

public sealed class BlobChangeLevelEvent : EntityEventArgs
{
    public EntityUid Station;
    public BlobStage Level;
}
