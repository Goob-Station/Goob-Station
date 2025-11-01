using System.Linq;
using Content.Goobstation.Common.SecondSkin;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Armor;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Clothing.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SecondSkin;

public abstract class SharedSecondSkinSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedItemSystem _itemSys = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;

    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly SharedContainerSystem Container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SecondSkinHolderComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SecondSkinHolderComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SecondSkinHolderComponent, EntInsertedIntoContainerMessage>(OnInsert);
        SubscribeLocalEvent<SecondSkinHolderComponent, EntRemovedFromContainerMessage>(OnRemove);
        SubscribeLocalEvent<SecondSkinHolderComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<SecondSkinHolderComponent, ActionActivateSecondSkin>(OnActivate);
        SubscribeLocalEvent<SecondSkinHolderComponent, GotUnequippedEvent>(OnUnequip);

        SubscribeLocalEvent<SecondSkinUserComponent, DamageModifyEvent>(OnModifyDamage);
        SubscribeLocalEvent<SecondSkinUserComponent, GetSecondSkinDeductionEvent>(OnGetDeduction);
        SubscribeLocalEvent<SecondSkinUserComponent, ComponentShutdown>(OnUserShutdown);
        SubscribeLocalEvent<SecondSkinUserComponent, MobStateChangedEvent>(OnUserStateChanged);
        SubscribeLocalEvent<SecondSkinUserComponent, AccumulateDisgustEvent>(OnAccumulateDisgust);

        SubscribeLocalEvent<SecondSkinComponent, ComponentShutdown>(OnSkinShutdown);
    }

    private void OnAccumulateDisgust(Entity<SecondSkinUserComponent> ent, ref AccumulateDisgustEvent args)
    {
        if (!Exists(ent.Comp.SecondSkin) || !TryComp(ent.Comp.SecondSkin, out SecondSkinComponent? secondSkin))
        {
            RemComp(ent, ent.Comp);
            return;
        }

        args.LevelIncrease += secondSkin.DisgustRate;
    }

    private void OnUserStateChanged(Entity<SecondSkinUserComponent> ent, ref MobStateChangedEvent args)
    {
        if (!Exists(ent.Comp.SecondSkin) || !TryComp(ent.Comp.SecondSkin, out SecondSkinComponent? comp))
        {
            RemComp(ent, ent.Comp);
            return;
        }

        DisableSecondSkin((ent.Comp.SecondSkin, comp), ent);
    }

    private void OnSkinShutdown(Entity<SecondSkinComponent> ent, ref ComponentShutdown args)
    {
        if (!Exists(ent.Comp.User) || TerminatingOrDeleted(ent.Comp.User.Value))
            return;

        DisableSecondSkin(ent, ent.Comp.User.Value, false);
    }

    private void OnUnequip(Entity<SecondSkinHolderComponent> ent, ref GotUnequippedEvent args)
    {
        var secondSkin = ent.Comp.Container.ContainedEntity;

        if (!TryComp(secondSkin, out SecondSkinComponent? comp))
        {
            RemComp<SecondSkinUserComponent>(args.Equipee);
            return;
        }

        DisableSecondSkin((secondSkin.Value, comp), args.Equipee);
    }

    private void OnGetDeduction(Entity<SecondSkinUserComponent> ent, ref GetSecondSkinDeductionEvent args)
    {
        var coverage = (BodyPartType) args.Coverage;
        var type = (TraumaType) args.TraumaType;

        if (!Exists(ent.Comp.SecondSkin) || !TryComp(ent.Comp.SecondSkin, out ArmorComponent? comp))
            return;

        if (!comp.ArmorCoverage.Contains(coverage))
            return;

        args.Deduction += comp.TraumaDeductions[type].Float();
    }

    private void OnModifyDamage(Entity<SecondSkinUserComponent> ent, ref DamageModifyEvent args)
    {
        if (args.TargetPart == null)
            return;

        if (!Exists(ent.Comp.SecondSkin) || !TryComp(ent.Comp.SecondSkin, out ArmorComponent? comp))
            return;

        var (partType, _) = _body.ConvertTargetBodyPart(args.TargetPart);

        if (!comp.ArmorCoverage.Contains(partType))
            return;

        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage,
            DamageSpecifier.PenetrateArmor(comp.Modifiers, args.Damage.ArmorPenetration));
    }

    private void OnActivate(Entity<SecondSkinHolderComponent> ent, ref ActionActivateSecondSkin args)
    {
        args.Handled = true;

        var secondSkin = ent.Comp.Container.ContainedEntity;

        if (!TryComp(secondSkin, out SecondSkinComponent? comp))
            return;

        if (comp.IsActive)
            DisableSecondSkin((secondSkin.Value, comp), args.Performer);
        else
            EnableSecondSkin((secondSkin.Value, comp), args.Performer);
    }

    protected void DisableSecondSkin(Entity<SecondSkinComponent> secondSkin, EntityUid performer, bool predicted = true)
    {
        if (secondSkin.Comp.User != performer)
            return;

        secondSkin.Comp.User = null;
        _audio.PlayPredicted(secondSkin.Comp.SoundUnequip, secondSkin, predicted ? performer : null);
        RemComp<SecondSkinUserComponent>(performer);
        Dirty(secondSkin);
    }

    private void EnableSecondSkin(Entity<SecondSkinComponent> secondSkin, EntityUid performer, bool predicted = true)
    {
        secondSkin.Comp.User = performer;
        _audio.PlayPredicted(secondSkin.Comp.SoundEquip, secondSkin, predicted ? performer : null);
        var user = EnsureComp<SecondSkinUserComponent>(performer);
        user.SecondSkin = secondSkin;
        Dirty(performer, user);
        Dirty(secondSkin);
        InitializeUser((performer, user), secondSkin);
    }

    private void InitializeUser(Entity<SecondSkinUserComponent> user, Entity<SecondSkinComponent> secondSkin)
    {
        if (!TryComp(user, out HumanoidAppearanceComponent? appearance))
            return;

        var species = _proto.Index(appearance.Species);
        var spriteSet = _proto.Index(species.SpriteSet);

        foreach (var layer in user.Comp.Layers)
        {
            string? id = null;
            if (spriteSet.Sprites.TryGetValue(layer, out var spriteLayer))
                id = spriteLayer;
            appearance.CustomBaseLayers[layer] = new CustomBaseLayerInfo(id, secondSkin.Comp.Color, user.Comp.Shader);
        }

        Dirty(user.Owner, appearance);
        UpdateSprite((user.Owner, appearance));
    }

    private void OnUserShutdown(Entity<SecondSkinUserComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        if (!TryComp(ent, out HumanoidAppearanceComponent? appearance))
            return;

        foreach (var layer in ent.Comp.Layers)
        {
            appearance.CustomBaseLayers.Remove(layer);
        }

        Dirty(ent.Owner, appearance);
        UpdateSprite((ent.Owner, appearance));
    }

    private void OnGetActions(Entity<SecondSkinHolderComponent> ent, ref GetItemActionsEvent args)
    {
        if (ent.Comp.Container.Count > 0 && args.SlotFlags == ent.Comp.Flags)
            args.AddAction(ref ent.Comp.SecondSkinAction, ent.Comp.SecondSkinActionId);
    }

    private void OnRemove(Entity<SecondSkinHolderComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ContainerId)
            return;

        var parent = Transform(ent).ParentUid;
        if (ent.Comp.SecondSkinAction != null && TryComp(parent, out ActionsComponent? actions))
            _actions.RemoveProvidedAction(parent, ent, ent.Comp.SecondSkinAction.Value, actions);

        Appearance.SetData(ent, SecondSkinKey.Equipped, false);

        UpdateClothing((ent, ent.Comp, null), null);

        if (!TryComp(args.Entity, out SecondSkinComponent? secondSkin))
            return;

        if (!TryComp(parent, out SecondSkinUserComponent? user) || user.SecondSkin != args.Entity)
        {
            secondSkin.User = null;
            Dirty(args.Entity, secondSkin);
            return;
        }

        DisableSecondSkin((args.Entity, secondSkin), parent);
    }

    private void OnInsert(Entity<SecondSkinHolderComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ContainerId)
            return;

        var parent = Transform(ent).ParentUid;
        if (ent.Comp.SecondSkinAction != null && TryComp(parent, out ActionsComponent? actions))
            _actions.AddAction((parent, actions), ent.Comp.SecondSkinAction.Value, ent.Owner);

        Appearance.SetData(ent, SecondSkinKey.Equipped, true);

        if (!TryComp(args.Entity, out SecondSkinComponent? secondSkin))
            return;

        secondSkin.User = null;
        Dirty(args.Entity, secondSkin);

        Appearance.SetData(ent, SecondSkinKey.Color, secondSkin.Color);
        UpdateClothing((ent, ent.Comp, null), secondSkin.Color);
    }

    private void UpdateClothing(Entity<SecondSkinHolderComponent, ClothingComponent?> ent, Color? color)
    {
        if (!Resolve(ent, ref ent.Comp2, false))
            return;

        var resPath = ent.Comp1.Sprite.RsiPath.ToString();
        PrototypeLayerData? layer = null;
        foreach (var data in ent.Comp2.ClothingVisuals.Values)
        {
            if (data.FirstOrDefault(x => x.RsiPath == resPath) is not { } found)
                continue;

            layer = found;
            break;
        }

        if (layer == null)
        {
            layer = new PrototypeLayerData()
            {
                RsiPath = ent.Comp1.Sprite.RsiPath.ToString(),
                State = ent.Comp1.State,
            };

            if (ent.Comp2.ClothingVisuals.TryGetValue(ent.Comp1.Slot, out var value))
                value.Add(layer);
            else
            {
                var defaultLayer = new PrototypeLayerData()
                {
                    RsiPath = ent.Comp2.RsiPath,
                    State = ent.Comp1.State,
                };

                ent.Comp2.ClothingVisuals[ent.Comp1.Slot] = new() { defaultLayer, layer };
            }
        }

        layer.Visible = color != null;
        layer.Color = color;
        Dirty(ent, ent.Comp2);
        _itemSys.VisualsChanged(ent);
    }

    private void OnMapInit(Entity<SecondSkinHolderComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.Container = Container.EnsureContainer<ContainerSlot>(ent, ent.Comp.ContainerId);
        _slots.AddItemSlot(ent, ent.Comp.ContainerId, ent.Comp.ItemSlot);
        _actionContainer.EnsureAction(ent, ref ent.Comp.SecondSkinAction, ent.Comp.SecondSkinActionId);
        Dirty(ent);
    }

    private void OnShutdown(Entity<SecondSkinHolderComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Container != default!)
            Container.ShutdownContainer(ent.Comp.Container);
        _actionContainer.RemoveAction(ent.Comp.SecondSkinAction);
    }

    protected virtual void UpdateSprite(Entity<HumanoidAppearanceComponent> ent) { }
}
