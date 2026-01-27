using Content.Client.UserInterface.Controls;
using Content.Goobstation.Client.UserInterface;
using Content.Goobstation.Shared.Cult.Magic;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Cult;
public sealed partial class CultBloodMagicBUI : EntityRadialMenuBUI
{
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    public CultBloodMagicBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override IEnumerable<RadialMenuOption> CreateModels(EntityUid owner)
    {
        owner = _player.LocalEntity!.Value;
        if (!_ent.TryGetComponent<BloodMagicProviderComponent>(owner, out var magic))
            yield break;

        var models = base.CreateModels(magic.KnownSpells);
        foreach (var model in models)
            yield return model;
    }
}
