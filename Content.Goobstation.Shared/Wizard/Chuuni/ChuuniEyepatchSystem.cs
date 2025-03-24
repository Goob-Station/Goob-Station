using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Magic.Components;
using Content.Shared.Verbs;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Wizard.Chuuni;

public sealed class ChuuniEyepatchSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly ClothingSystem _clothing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChuuniEyepatchComponent, GetVerbsEvent<AlternativeVerb>>(AddFlipVerb);
        SubscribeLocalEvent<ChuuniEyepatchComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ChuuniEyepatchComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<ChuuniEyepatchComponent, InventoryRelayedEvent<GetSpellInvocationEvent>>(OnGetInvocation);
        SubscribeLocalEvent<ChuuniEyepatchComponent, InventoryRelayedEvent<GetMessagePostfixEvent>>(OnGetPostfix);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        var query = EntityQueryEnumerator<ChuuniEyepatchComponent>();
        while (query.MoveNext(out var uid, out var eyepatch))
        {
            if (eyepatch.Accumulator >= eyepatch.Delay)
                continue;

            eyepatch.Accumulator += frameTime;

            if (eyepatch.Accumulator >= eyepatch.Delay)
                Dirty(uid, eyepatch);
        }
    }

    private void OnGetPostfix(Entity<ChuuniEyepatchComponent> ent,
        ref InventoryRelayedEvent<GetMessagePostfixEvent> args)
    {
        args.Args.Postfix = ent.Comp.MessagePostfix;
    }

    private void OnGetInvocation(Entity<ChuuniEyepatchComponent> ent,
        ref InventoryRelayedEvent<GetSpellInvocationEvent> args)
    {
        args.Args.Invocation = ent.Comp.Invocations[args.Args.School];

        var performer = args.Args.Performer;
        if (!TryComp(performer, out DamageableComponent? damageable))
            return;

        if (!ent.Comp.CanHeal || damageable.TotalDamage <= FixedPoint2.Zero)
            return;

        ent.Comp.Accumulator = 0f;
        Dirty(ent);

        if (ent.Comp.HealAmount < damageable.TotalDamage)
            args.Args.ToHeal = damageable.Damage * ent.Comp.HealAmount / damageable.TotalDamage;
        else
            args.Args.ToHeal = damageable.Damage;
    }

    private void OnExamine(Entity<ChuuniEyepatchComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.SelectedBackstory != null)
            args.PushMarkup(Loc.GetString(ent.Comp.SelectedBackstory.Value));
    }

    private void OnInit(Entity<ChuuniEyepatchComponent> ent, ref ComponentInit args)
    {
        var (uid, comp) = ent;

        _appearance.SetData(uid, FlippedVisuals.Flipped, comp.IsFliped);
        _clothing.SetEquippedPrefix(uid, comp.IsFliped ? comp.FlippedPrefix : null);

        if (_net.IsClient || comp.Backstories.Count == 0)
            return;

        comp.SelectedBackstory = _random.Pick(comp.Backstories);
        Dirty(uid, comp);
    }

    private void AddFlipVerb(Entity<ChuuniEyepatchComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        var (uid, comp) = ent;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                comp.IsFliped = !comp.IsFliped;
                _clothing.SetEquippedPrefix(uid, comp.IsFliped ? comp.FlippedPrefix : null);
                _appearance.SetData(uid, FlippedVisuals.Flipped, comp.IsFliped);
                Dirty(ent);
            },
            Text = Loc.GetString("flippable-verb-get-data-text"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/flip.svg.192dpi.png")),
            Priority = 1,
        };

        args.Verbs.Add(verb);
    }
}

public sealed class GetSpellInvocationEvent(MagicSchool school, EntityUid performer)
    : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.EYES;

    public MagicSchool School = school;

    public EntityUid Performer = performer;

    public DamageSpecifier ToHeal = new();

    public LocId? Invocation;
}

public sealed class GetMessagePostfixEvent : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.EYES;

    public string Postfix = string.Empty;
}
