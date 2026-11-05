using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.MartialArts;

// TODO: scarp scroll isnt here because special behavior and im lazy
public sealed partial class MartialArtsGrantSystem : EntitySystem
{
    [Dependency] private MartialArtsSystem _martialArts = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        // me when cant subscribe to abstract component
        SubscribeMartialArtGrant<GrantCqcComponent>();
        SubscribeMartialArtGrant<GrantCorporateJudoComponent>();
        SubscribeMartialArtGrant<GrantCapoeiraComponent>();
        SubscribeMartialArtGrant<GrantHellRipComponent>();
        SubscribeMartialArtGrant<GrantKungFuDragonComponent>();
        SubscribeMartialArtGrant<GrantNinjutsuComponent>();
        //SubscribeMartialArtGrant<GrantSleepingCarpComponent>(); // TODO : scroll doesnt actually use this system yet
    }

    private void SubscribeMartialArtGrant<T>()
        where T : GrantMartialArtKnowledgeComponent
    {
        SubscribeLocalEvent<T, MapInitEvent>(OnGrantMapInitEvent);
        SubscribeLocalEvent<T, ComponentShutdown>(OnGrantShutdownEvent);
        SubscribeLocalEvent<T, UseInHandEvent>(OnGrantUseInHand);
        SubscribeLocalEvent<T, ClothingGotEquippedEvent>(OnGrantEquipped);
        SubscribeLocalEvent<T, ClothingGotUnequippedEvent>(OnGrantUnequipped);
    }

    private void OnGrantMapInitEvent<T>(Entity<T> ent, ref MapInitEvent args)
        where T : GrantMartialArtKnowledgeComponent
    {
        // TODO: kinda slop
        // i want to ignore martialartsgranttype so you can just add the comp to people regardless ,
        // but have to make sure we dont try to give a book martial arts and shit
        if (!HasComp<MeleeWeaponComponent>(ent) && !HasComp<MobStateComponent>(ent))
            return;

        _martialArts.TryGrantMartialArt(ent.Owner, _proto.Index(ent.Comp.MartialArtsForm), ent.Comp.RestrictedZones);
    }

    private void OnGrantShutdownEvent<T>(Entity<T> ent, ref ComponentShutdown args)
        where T : GrantMartialArtKnowledgeComponent
    {
        // TODO: kinda slop
        // i want to ignore martialartsgranttype so you can just add the comp to people regardless ,
        // but have to make sure we dont try to give a book martial arts and shit
        if (!HasComp<MeleeWeaponComponent>(ent) && !HasComp<MobStateComponent>(ent))
            return;

        _martialArts.TryRemoveMartialArt(ent.Owner, _proto.Index(ent.Comp.MartialArtsForm));
    }

    private void OnGrantUseInHand<T>(Entity<T> ent, ref UseInHandEvent args)
        where T : GrantMartialArtKnowledgeComponent
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (!ent.Comp.GrantType.HasFlag(MartialArtsGrantType.Use) || args.Handled)
            return;

        args.Handled = true;

        if (!_martialArts.TryGrantMartialArt(args.User, _proto.Index(ent.Comp.MartialArtsForm), ent.Comp.RestrictedZones))
            return;

        var coords = Transform(args.User).Coordinates;
        _audio.PlayPredicted(ent.Comp.SoundOnUse, coords, ent);

        if (ent.Comp.MultiUse)
            return;

        if (ent.Comp.LearnMessage is { } learnLoc)
            _popup.PopupClient(Loc.GetString(learnLoc), ent);

        PredictedSpawnAtPosition(ent.Comp.SpawnedProto, coords);
        QueueDel(ent);
    }

    private void OnGrantEquipped<T>(Entity<T> ent, ref ClothingGotEquippedEvent args)
        where T : GrantMartialArtKnowledgeComponent
    {
        if (!ent.Comp.GrantType.HasFlag(MartialArtsGrantType.Clothing))
            return;

        _martialArts.TryGrantMartialArt(args.Wearer, _proto.Index(ent.Comp.MartialArtsForm), ent.Comp.RestrictedZones);
    }

    private void OnGrantUnequipped<T>(Entity<T> ent, ref ClothingGotUnequippedEvent args)
        where T : GrantMartialArtKnowledgeComponent
    {
        if (!ent.Comp.GrantType.HasFlag(MartialArtsGrantType.Clothing))
            return;

        _martialArts.TryRemoveMartialArt(args.Wearer, _proto.Index(ent.Comp.MartialArtsForm));
    }
}