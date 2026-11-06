using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Goobstation.Security;

/// <summary>
/// Searches for entity that passes whitelist when clicking on it, then begins a do-after and deals damage to it on completion.
/// </summary>

public sealed partial class StructureBreakerSystem : EntitySystem
{
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StructureBreakerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<StructureBreakerComponent, StructureBreakerDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(Entity<StructureBreakerComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not { } target)
            return;

        if (!_whitelist.IsWhitelistPass(ent.Comp.Whitelist, target))
            return;

        var doAfter = new DoAfterArgs(EntityManager, args.User, ent.Comp.Delay, new StructureBreakerDoAfterEvent(), ent, target, ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnHandChange = true,
            NeedHand = true,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
            return;

        args.Handled = true;
    }

    private void OnDoAfter(Entity<StructureBreakerComponent> ent, ref StructureBreakerDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Target is not { } target)
            return;

        _damageable.TryChangeDamage(target, ent.Comp.Damage, origin: args.User);
        _audio.PlayPredicted(ent.Comp.HitSound, target, args.User);

        args.Handled = true;
    }
}
