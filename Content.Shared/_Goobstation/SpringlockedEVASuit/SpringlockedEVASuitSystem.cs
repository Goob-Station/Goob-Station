using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Robust.Shared.Network;
using Content.Shared.Clothing.Components;
using Robust.Shared.Serialization;
using Content.Shared.Interaction.Components;
using Content.Shared.Verbs;
using Content.Shared.Item;
using Content.Shared.Movement.Systems;

namespace Content.Shared._Goobstation.SpringlockedEVASuit;

public sealed class SpringlockedEVASuitSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpringlockedEVASuitComponent, ToggleClothingAttemptEvent>(OnToggleAttempt);
        SubscribeLocalEvent<SpringlockedEVASuitComponent, GetVerbsEvent<Verb>>(OnVerbAdd);

        // DoAfter event handlers
        SubscribeLocalEvent<SpringlockedEVASuitComponent, SpringlockedEVARemoveDoAfterEvent>(OnRemove);
    }

    private void OnToggleAttempt(EntityUid uid, SpringlockedEVASuitComponent trap, ref ToggleClothingAttemptEvent args)
    {
        if (args.Cancelled)
        {
            return;
        }
        if (trap.IsSpringed)
        {
            args.Cancel();
            return;
        }

        trap.IsSpringed = true;
        trap.Wearer = Transform(uid).ParentUid;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Piercing", 45);
        _damageable.TryChangeDamage(trap.Wearer, damage, true, origin: uid);
        if (_net.IsServer)
        {
            _audio.PlayPredicted(trap.SnapSound, trap.Wearer.Value, null);
            _popup.PopupEntity("The suit springs shut!", trap.Wearer.Value, trap.Wearer.Value, PopupType.LargeCaution);
        }

        EnsureComp<UnremoveableComponent>(uid);

        //_appearance.SetData(uid, SpringlockedEVAState.State, "equipped-OUTERCLOTHING-springed"); // fix sprite not changing.

        _movementSpeedModifierSystem.ChangeBaseSpeed(uid, 0.5f, 0.5f, 20);

        _metaDataSystem.SetEntityName(uid, "Springlocked EVA suit");
        _metaDataSystem.SetEntityDescription(uid, "A heavily modified EVA suit turned into a gruesome trap. Multiple sharp mechanisms are embedded into the fabric, causing continuous damage to the wearer.");

        //need someway to remove the action
        //RemComp<ToggleableClothing>(uid); // unaccesable from Content.Shared
        //RemComp<ActionsContainerComponent>(uid); // this causes an exception (metadata component does not exit on target) to fire when right clicking a springed victim

        Dirty(uid, trap);
    }

    private void OnVerbAdd(EntityUid uid, SpringlockedEVASuitComponent trap, GetVerbsEvent<Verb> args)
    {
        if (!_actionBlockerSystem.CanComplexInteract(args.User)
            || !trap.IsSpringed
            || trap.Wearer == args.User)
        {
            return;
        }

        args.Verbs.Add(new Verb()
        {
            DoContactInteraction = true,
            Text = "Remove trap",
            Act = () =>
            {
                var doAfterArgs = new DoAfterArgs(EntityManager, args.User, 3f,
                    new SpringlockedEVARemoveDoAfterEvent(), uid, uid)
                {
                    BreakOnDamage = true,
                    BreakOnMove = true,
                    AttemptFrequency = AttemptFrequency.StartAndEnd
                };

                _doAfter.TryStartDoAfter(doAfterArgs);
            }
        });
    }

    private void OnRemove(EntityUid uid, SpringlockedEVASuitComponent component, ref SpringlockedEVARemoveDoAfterEvent args)
    {
        if (!component.IsSpringed)
        {
            return;
        }
        component.IsSpringed = false;
        RemComp<UnremoveableComponent>(uid);
        if (component.Wearer != null && TryComp<ItemComponent>(uid, out var _))
        {
            _inventory.TryUnequip(component.Wearer.Value, "outerClothing", true, true);
        }
        component.Wearer = null;

        RemComp<ClothingComponent>(uid);

        Dirty(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SpringlockedEVASuitComponent>();
        while (query.MoveNext(out var uid, out var trap))
        {
            if (trap.IsSpringed == false || trap.Wearer == null)
            {
                continue;
            }
            trap.UpdateTimer += frameTime;

            if (trap.UpdateTimer >= trap.UpdateDelay)
            {
                _damageable.TryChangeDamage(trap.Wearer.Value, SpringedPassiveDamage, true, origin: uid);

                trap.UpdateTimer = 0;
            }
        }
    }

    private static readonly DamageSpecifier SpringedPassiveDamage = new DamageSpecifier()
    { DamageDict = new() { { "Piercing", 0.5f } } };

    [Serializable, NetSerializable]
    private sealed partial class SpringlockedEVARemoveDoAfterEvent : SimpleDoAfterEvent { }
}
