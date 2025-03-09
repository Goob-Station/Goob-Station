using Content.Server.GameTicking.Rules.Components;

namespace Content.Server._Goobstation.Blob;

public sealed class BlobChangeLevelEvent : EntityEventArgs
{
    public EntityUid Station;
    public BlobStage Level;
}
