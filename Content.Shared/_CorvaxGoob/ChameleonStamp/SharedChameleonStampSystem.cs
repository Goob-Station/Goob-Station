using Content.Shared.Paper;
using Content.Shared.Prototypes;
using Robust.Shared.Prototypes;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared._CorvaxGoob.ChameleonStamp;

public abstract class SharedChameleonStampSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private List<ProtoId<EntityPrototype>> _stampsPresets = new();

    protected void UpdatePresets()
    {
        _stampsPresets.Clear();

        var prototypes = _proto.EnumeratePrototypes<EntityPrototype>();

        foreach (var proto in prototypes)
        {
            if (proto.Abstract || proto.HideSpawnMenu || !proto.HasComponent<StampComponent>())
                continue;

            _stampsPresets.Add(proto);
        }
    }

    public bool ValidatePreset(EntProtoId preset, [NotNullWhen(true)] out EntityPrototype? presetPrototype, [NotNullWhen(true)] out StampComponent? presetStampComponent)
    {
        presetPrototype = null;
        presetStampComponent = null;

        if (!_stampsPresets.Contains(preset.Id)
            || !_proto.TryIndex<EntityPrototype>(preset, out presetPrototype)
        || !presetPrototype.TryGetComponent<StampComponent>(out presetStampComponent))
            return false;

        return true;
    }

    public List<ProtoId<EntityPrototype>> GetAllPresets() => _stampsPresets;
}
