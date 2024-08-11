using Content.Server.Heretic.Components;
using Content.Server.Objectives.Components;
using Content.Server.Revolutionary.Components;
using Content.Shared.Changeling;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Heretic;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using System.Text;

namespace Content.Server.Heretic;

public sealed partial class HereticRitualSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly HereticKnowledgeSystem _knowledge = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly HereticSystem _heretic = default!;

    public SoundSpecifier RitualSuccessSound = new SoundPathSpecifier("/Audio/Goobstation/Heretic/castsummon.ogg");

    public HereticRitualPrototype GetRitual(ProtoId<HereticRitualPrototype>? id)
    {
        if (id == null) throw new ArgumentNullException();
        return _proto.Index<HereticRitualPrototype>(id);
    }

    public bool TryDoRitual(EntityUid performer, EntityUid platform, ProtoId<HereticRitualPrototype> ritualId)
    {
        if (!TryComp<HereticComponent>(performer, out var hereticComp))
            return false;

        var rit = (HereticRitualPrototype) GetRitual(ritualId).Clone();
        var lookup = _lookup.GetEntitiesInRange(platform, 0.5f);

        var missingList = new List<string>();
        var toDelete = new List<EntityUid>();

        // is it a sacrifice?
        if (rit.OutputEvent != null && rit.OutputEvent.GetType() == typeof(HereticRitualSacrificeEvent))
        {
            EntityUid? acc = null;

            foreach (var look in lookup)
            {
                // get the first dead one
                if (!TryComp<MobStateComponent>(look, out var mobstate)
                || !HasComp<HumanoidAppearanceComponent>(look))
                    continue;

                // eldritch gods don't want these nature freaks
                if (HasComp<ChangelingComponent>(look))
                    continue;

                if (mobstate.CurrentState == Shared.Mobs.MobState.Dead)
                {
                    acc = look;

                    if (TryComp<DamageableComponent>(look, out var dmg))
                    {
                        // YES!!! GIB!!!
                        var prot = (ProtoId<DamageGroupPrototype>) "Blunt";
                        var dmgtype = _proto.Index(prot);
                        _damage.TryChangeDamage(look, new DamageSpecifier(dmgtype, 500), true);
                    }

                    break;
                }
            }

            if (acc == null)
            {
                _popup.PopupEntity(Loc.GetString("heretic-ritual-fail-sacrifice"), performer, performer);
                return false;
            }

            var knowledgeGain = HasComp<CommandStaffComponent>(acc) ? 2f : 1f;

            if (_mind.TryGetMind(performer, out var mindId, out var mind))
                if (_mind.TryGetObjectiveComp<HereticSacrificeConditionComponent>(mindId, out var objective, mind))
                {
                    if (HasComp<CommandStaffComponent>(acc) && objective.IsCommand)
                        objective.Sacrificed += 1;
                    objective.Sacrificed += 1; // give one nontheless
                }

            _heretic.UpdateKnowledge(performer, hereticComp, knowledgeGain);
            return true;
        }

        // check for matching entity names
        if (rit.RequiredEntityNames != null)
        {
            foreach (var name in rit.RequiredEntityNames)
            {
                foreach (var look in lookup)
                {
                    if (Name(look) == name.Key)
                    {
                        rit.RequiredEntityNames[name.Key] -= 1;
                        toDelete.Add(look);
                    }
                }
            }

            foreach (var name in rit.RequiredEntityNames)
                if (name.Value > 0)
                    missingList.Add(name.Key);
        }

        // check for matching tags
        if (rit.RequiredTags != null)
        {
            foreach (var tag in rit.RequiredTags)
            {
                foreach (var look in lookup)
                {
                    if (!TryComp<TagComponent>(look, out var tags))
                        continue;
                    var ltags = tags.Tags;

                    if (ltags.Contains(tag.Key))
                    {
                        rit.RequiredTags[tag.Key] -= 1;
                        toDelete.Add(look);
                    }
                }
            }

            foreach (var tag in rit.RequiredTags)
                if (tag.Value > 0)
                    missingList.Add(tag.Key);
        }

        // are we missing anything?
        if (missingList.Count > 0)
        {
            // we are! notify the performer about that!
            var sb = new StringBuilder();
            foreach (var missing in missingList)
                sb.Append(missing);

            _popup.PopupEntity(Loc.GetString("heretic-ritual-fail-items", ("itemlist", sb.ToString())), performer, performer);
            return false;
        }

        // yay! ritual successfull!
        // ya get some, ya lose some
        foreach (var ent in toDelete)
            QueueDel(ent);

        // add stuff
        if (rit.Output != null && rit.Output.Count > 0)
        {
            foreach (var ent in rit.Output.Keys)
                for (int i = 0; i < rit.Output[ent]; i++)
                    Spawn(ent, Transform(platform).Coordinates);
        }
        if (rit.OutputEvent != null)
            RaiseLocalEvent(rit.OutputEvent);

        if (rit.OutputKnowledge != null)
            _knowledge.AddKnowledge(performer, hereticComp, (ProtoId<HereticKnowledgePrototype>) rit.OutputKnowledge);

        return true;
    }
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticRitualRuneComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<HereticRitualRuneComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteract(Entity<HereticRitualRuneComponent> ent, ref InteractHandEvent args)
    {
        if (!TryComp<HereticComponent>(args.User, out var heretic))
            return;

        if (heretic.KnownRituals.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("heretic-ritual-norituals"), args.User, args.User);
            return;
        }

        if (heretic.ChosenRitual == null)
            heretic.ChosenRitual = heretic.KnownRituals[0];

        else if (heretic.ChosenRitual != null)
        {
            var index = heretic.KnownRituals.FindIndex(m => m == heretic.ChosenRitual) + 1;

            if (index >= heretic.KnownRituals.Count)
                index = 0;

            heretic.ChosenRitual = heretic.KnownRituals[index];
        }

        var ritualName = Loc.GetString(GetRitual(heretic.ChosenRitual).Name);
        _popup.PopupEntity(Loc.GetString("heretic-ritual-switch", ("name", ritualName)), args.User, args.User);
    }
    private void OnInteractUsing(Entity<HereticRitualRuneComponent> ent, ref InteractUsingEvent args)
    {
        if (!TryComp<HereticComponent>(args.User, out var heretic))
            return;

        if (!TryComp<MansusGraspComponent>(args.Used, out var grasp))
            return;

        if (heretic.ChosenRitual == null)
        {
            _popup.PopupEntity(Loc.GetString("heretic-ritual-noritual"), args.User, args.User);
            return;
        }

        if (!TryDoRitual(args.User, ent, (ProtoId<HereticRitualPrototype>) heretic.ChosenRitual))
            return;

        _audio.PlayPvs(RitualSuccessSound, ent, AudioParams.Default.WithVolume(-3f));
        Spawn("HereticRuneRitualAnimation", Transform(ent).Coordinates);
    }
}
