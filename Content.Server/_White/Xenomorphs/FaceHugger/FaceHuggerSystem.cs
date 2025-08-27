using Content.Server.Body.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.Whitelist;

namespace Content.Server._White.Xenomorphs.FaceHugger;

public sealed class FaceHuggerSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FaceHuggerComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<FaceHuggerComponent, GotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnGotEquipped(EntityUid uid, FaceHuggerComponent component, GotEquippedEvent args)
    {
        if (args.Slot != component.Slot
            || component.LarvaEmbryoCount <= 0
            || !_mobState.IsAlive(uid)
            || _entityWhitelist.IsBlacklistPass(component.Blacklist, args.Equipee)
            || _body.GetRootPartOrNull(args.Equipee) is not {} rootPart)
            return;

        var organ = Spawn(component.InfectionPrototype);
        _body.TryCreateOrganSlot(rootPart.Entity, component.InfectionSlotId, out _, rootPart.BodyPart);

        if (!_body.InsertOrgan(rootPart.Entity, organ, component.InfectionSlotId, rootPart.BodyPart))
        {
            QueueDel(organ);
            return;
        }

        component.LarvaEmbryoCount--;
    }

    private void OnGotUnequipped(EntityUid uid, FaceHuggerComponent component, GotUnequippedEvent args)
    {
        if (component.LarvaEmbryoCount > 0 || HasComp<PassiveDamageComponent>(uid))
            return;

        var passiveDamage = EnsureComp<PassiveDamageComponent>(uid);
        passiveDamage.AllowedStates = component.AllowedPassiveDamageStates;
        passiveDamage.Damage = component.PassiveDamage;
    }
}
