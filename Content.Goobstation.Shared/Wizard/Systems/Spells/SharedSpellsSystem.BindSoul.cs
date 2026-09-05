using Content.Goobstation.Shared.Wizard.Events;
using Content.Goobstation.Shared.Wizard.Components;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Construction.Components;
using Content.Shared.Ghost;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Components;
using Content.Shared.Item;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Roles.Components;
using Content.Shared.Tag;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects.Components.Localization;
using Content.Shared._Goobstation.Wizard;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private LocId _locFailPhylacterySoulNotBound = "spell-fail-soul-not-bound";
    private LocId _locFailPhylacteryItemDestroyed = "spell-fail-item-destroyed";
    private LocId _locFailPhylacteryItemOnOtherPlane = "spell-fail-item-on-another-plane";
    private LocId _locFailPhylacteryNoHeldEntity = "spell-fail-no-held-entity";
    private LocId _locFailPhylacteryNoSoul = "spell-fail-no-soul";
    private LocId _locFailPhylacterySilicon = "spell-fail-bind-soul-silicon";
    private LocId _locFailPhylacteryUnremovable = "spell-fail-unremoveable";
    private LocId _locFailPhylacteryBlacklist = "spell-fail-soul-item-not-suitable";

    private void OnBindSoul(BindSoulEvent ev)
    {
        if (ev.Handled)
            return;

        if (_mobState.IsCritical(ev.Performer))
            return;

        if (!_mind.TryGetMind(ev.Performer, out var mind, out var mindComponent))
            return;

        TryComp<SoulBoundComponent>(mind, out var soulBound);

        if (_mind.IsCharacterDeadIc(mindComponent))
        {
            if (soulBound == null)
            {
                _popup.PopupClient(Loc.GetString(_locFailPhylacterySoulNotBound), ev.Performer);
                return;
            }

            if (!HasComp<PhylacteryComponent>(soulBound.Item))
            {
                _popup.PopupClient(Loc.GetString(_locFailPhylacteryItemDestroyed), ev.Performer);
                return;
            }

            if (Transform(soulBound.Item.Value).MapUid == null ||
                Transform(soulBound.Item.Value).MapUid != soulBound.MapId)
            {
                _popup.PopupClient(Loc.GetString(_locFailPhylacteryItemOnOtherPlane), ev.Performer);
                return;
            }

            _bindSoul.Resurrect(mind, soulBound.Item.Value, mindComponent, soulBound);
            ev.Handled = true;
            return;
        }

        if (HasComp<GhostComponent>(ev.Performer))
            return;

        if (soulBound != null)
        {
            _popup.PopupClient(Loc.GetString(_locFailPhylacteryNoSoul), ev.Performer);
            return;
        }

        if (!_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (HasComp<SiliconComponent>(ev.Performer))
        {
            _popup.PopupClient(Loc.GetString(_locFailPhylacterySilicon), ev.Performer);
            return;
        }

        if (!_hands.TryGetActiveItem(ev.Performer, out var item))
        {
            _popup.PopupClient(Loc.GetString(_locFailNoHeldEntity), ev.Performer);
            return;
        }

        if (HasComp<UnremoveableComponent>(item) || !HasComp<ItemComponent>(item))
        {
            _popup.PopupClient(Loc.GetString(_locFailPhylacteryUnremovable, ("item", item)), ev.Performer);
            return;
        }

        if (_whitelist.IsValid(ev.Blacklist, item))
        {
            _popup.PopupClient(Loc.GetString(_locFailPhylacteryBlacklist, ("item", item)), ev.Performer);
            return;
        }

        BindSoul(ev, item.Value, mind, mindComponent);
        ev.Handled = true;
    }

    private void BindSoul(BindSoulEvent ev, EntityUid item, EntityUid mind, MindComponent mindComponent)
    {
        var oldEnt = ev.Performer;
        var xform = Transform(oldEnt);
        var meta = MetaData(oldEnt);

        var mapId = xform.MapUid;

        var newEntity = PredictedSpawnAtPosition(ev.Entity,
            xform.Coordinates);
        _xform.SetWorldRotation(newEntity, _xform.GetWorldRotation(oldEnt));

        if (_container.TryGetContainingContainer((oldEnt, xform, meta), out var cont))
            _container.Insert(newEntity, cont);

        var name = meta.EntityName;

        _meta.SetEntityName(newEntity, name);

        int? age = null;
        Gender? gender = null;
        Sex? sex = null;
        if (TryComp(oldEnt, out HumanoidAppearanceComponent? humanoid))
        {
            age = humanoid.Age;
            gender = humanoid.Gender;
            sex = humanoid.Sex;
            if (TryComp(newEntity, out HumanoidAppearanceComponent? newHumanoid))
            {
                newHumanoid.Age = age.Value;
                newHumanoid.Gender = gender.Value;
                newHumanoid.Sex = sex.Value;
                Dirty(newEntity, newHumanoid);
                if (TryComp(newEntity, out GrammarComponent? grammar))
                    _grammar.SetGender((newEntity, grammar), gender.Value);
                var identity = Identity.Entity(newEntity, EntityManager);
                if (TryComp(identity, out GrammarComponent? identityGrammar))
                    _grammar.SetGender((identity, identityGrammar), gender.Value);
            }
        }

        _identity.QueueIdentityUpdate(newEntity);

        _mind.TransferTo(mind, newEntity, mind: mindComponent);

        _faction.ClearFactions(newEntity, false);
        _faction.AddFaction(newEntity, "Wizard");
        RemCompDeferred<TransferMindOnGibComponent>(newEntity);
        EnsureComp<WizardComponent>(newEntity);
        if (!_role.MindHasRole<WizardRoleComponent>(mind, out _))
            _role.MindAddRole(mind, "MindRoleWizard", mindComponent, true);

        EnsureComp<PhylacteryComponent>(item);
        _item.SetSize(item, ev.PhylacterySize);
        RemCompDeferred<TagComponent>(item);
        RemCompDeferred<AnchorableComponent>(item);

        var soulBound = EntityManager.ComponentFactory.GetComponent<SoulBoundComponent>();
        soulBound.Name = name;
        soulBound.Item = item;
        soulBound.MapId = mapId;
        soulBound.Age = age;
        soulBound.Gender = gender;
        soulBound.Sex = sex;
        AddComp(mind, soulBound, true);

        // due to _inventory.TransferEntityInventories, the rest is on still on server still :c
        BindSoulRelay(ev, oldEnt, newEntity, mindComponent);
    }

    protected virtual void BindSoulRelay(BindSoulEvent ev, EntityUid oldEnt, EntityUid newEntity, MindComponent mindComponen) { }
}