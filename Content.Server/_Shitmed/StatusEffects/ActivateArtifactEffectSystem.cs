using Content.Shared._Shitmed.StatusEffects;
using Content.Server.Xenoarchaeology.XenoArtifacts;

namespace Content.Server._Shitmed.StatusEffects;

public sealed class ActivateArtifactEffectSystem : EntitySystem
{
    [Dependency] private readonly ArtifactSystem _artifact = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<ActivateArtifactEffectComponent, ComponentInit>(OnInit);
    }
    private void OnInit(EntityUid uid, ActivateArtifactEffectComponent component, ComponentInit args)
    {
        if (!TryComp<ArtifactComponent>(uid, out var artifact))
            return;

        _artifact.TryActivateArtifact(uid, logMissing: false);
    }


}

