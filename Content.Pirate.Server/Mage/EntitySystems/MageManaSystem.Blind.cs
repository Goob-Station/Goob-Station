using System.Threading.Tasks;
using Content.Server.Magic;
using Content.Pirate.Shared.Mage.Components;
using Content.Pirate.Shared.Mage.Events;
using Content.Shared.Actions;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Storage.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Server.Mage.EntitySystems;

public sealed class MageBlindSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BlindableSystem _blindnessSystem = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly MagicSystem _magic = default!;
    [Dependency] private readonly MageManaSystem _mana = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    //[Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MageBlindSpellEvent>(OnBlindSpell);
    }

    /// <summary>
    ///     Teleports the user to the clicked location
    /// </summary>
    /// <param name="args"></param>
    private async void OnBlindSpell(MageBlindSpellEvent args)
    {
        if (!_entity.TryGetComponent<MageComponent>(args.Performer, out var comp) || // Not a Mage
            _entity.TryGetComponent<HandcuffComponent>(args.Performer, out var cuffs) || // handcuffed
            _entity.HasComponent<InsideEntityStorageComponent>(args.Performer)) // Inside an entity storage
            return;

        if (args.Handled)
            return;

        var transform = Transform(args.Performer);
        var coords = transform.Coordinates;

        if (!_mana.TryUseAbility(args.Performer, comp, args.ManaCost))
            return;

        var comps = Comp<BlindableComponent>(args.Target);
        var olddamage = comps.EyeDamage;
        _blindnessSystem.AdjustEyeDamage(args.Target, 9);
        _blindnessSystem.UpdateIsBlind(args.Target);
        await Task.Delay(10000);
        _blindnessSystem.AdjustEyeDamage(args.Target, -(9 - olddamage));
        _blindnessSystem.UpdateIsBlind(args.Target);

        //_magic.Speak(args);

        args.Handled = true;
    }
}
