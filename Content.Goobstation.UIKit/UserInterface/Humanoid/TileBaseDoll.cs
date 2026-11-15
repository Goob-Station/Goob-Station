using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Preferences;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.UIKit.UserInterface.Humanoid;

public sealed class TileBaseDoll : IDisposable
{
    private static readonly TimeSpan ApplyInterval = TimeSpan.FromSeconds(0.2);

    private readonly IEntityManager _entMan;
    private readonly IPrototypeManager _proto;

    private string? _species;
    private HumanoidCharacterProfile? _pending;
    private TimeSpan _nextApply;

    public EntityUid? Entity { get; private set; }

    public TileBaseDoll(IEntityManager entMan, IPrototypeManager proto)
    {
        _entMan = entMan;
        _proto = proto;
    }

    public bool EnsureSpecies(string species)
    {
        if (_species == species && Entity != null)
            return false;

        Delete();
        _species = species;
        _nextApply = TimeSpan.Zero;

        if (_proto.TryIndex<SpeciesPrototype>(species, out var speciesProto))
            Entity = _entMan.SpawnEntity(speciesProto.DollPrototype, MapCoordinates.Nullspace);

        return true;
    }

    public void Queue(HumanoidCharacterProfile profile)
    {
        _pending = profile.WithHeight(1f).WithWidth(1f);
    }

    public void Update(TimeSpan now)
    {
        if (_pending is not { } profile || Entity is not { } doll || now < _nextApply)
            return;

        if (profile.Species.Id != _species)
            return;

        _pending = null;
        _nextApply = now + ApplyInterval;
        _entMan.System<SharedHumanoidAppearanceSystem>().LoadProfile(doll, profile);
    }

    public void Dispose()
    {
        Delete();
    }

    private void Delete()
    {
        if (Entity is { } doll)
            _entMan.DeleteEntity(doll);

        Entity = null;
    }
}
