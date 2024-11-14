using Content.Server.GameTicking.Rules.Components;
using Content.Shared.Blob.Components;

namespace Content.Server.Blob;

public sealed class BlobChangeLevelEvent : EntityEventArgs
{
    public EntityUid Station;
    public BlobStage Level;
}
