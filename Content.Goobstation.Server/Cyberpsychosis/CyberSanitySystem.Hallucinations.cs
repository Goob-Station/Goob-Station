using Content.Goobstation.Shared.CyberSanity;
using Content.Server.Emoting.Components;
using Content.Server.Ghost.Roles.Components;
using Content.Shared.Actions.Components;
using Content.Shared.Database;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;

namespace Content.Goobstation.Server.Cyberpsychosis;

public sealed partial class CyberSanitySystem
{
    private void InitializeHallucination()
    {
        SubscribeLocalEvent<CyberPsychosisEntityComponent, MapInitEvent>(OnPsychoEntityInit);
        SubscribeLocalEvent<CyberPsychosisEntityComponent, CyberPsychoTeleportToOwnerActionEvent>(OnTeleport);
        SubscribeLocalEvent<CyberPsychosisEntityComponent, CyberPsychoForceSayActionEvent>(OnForceSay);
    }

    private void OnPsychoEntityInit(Entity<CyberPsychosisEntityComponent> ent, ref MapInitEvent args)
    {

    }

    private void OnTeleport(Entity<CyberPsychosisEntityComponent> ent, ref CyberPsychoTeleportToOwnerActionEvent args)
    {
        var msg = new StaticFlashEffectMessage(2f);
        RaiseNetworkEvent(msg, ent.Owner);
        RaiseNetworkEvent(msg, ent.Comp.PsychosisOwner);

        _transform.SetCoordinates(ent.Owner, Transform(ent.Comp.PsychosisOwner).Coordinates);
    }

    private void OnForceSay(Entity<CyberPsychosisEntityComponent> ent, ref CyberPsychoForceSayActionEvent args)
    {
        if (!_player.TryGetSessionByEntity(ent.Owner, out var session))
            return;

        _quickDialog.OpenDialog(session, Loc.GetString("cyber-psycho-force-say-title"), "Message",
        (string arg) =>
        {
            if (!ent.Comp.SpeechAction.HasValue || _actions.IsCooldownActive(Comp<ActionComponent>(ent.Comp.SpeechAction.Value), _timing.CurTime))
                return;

            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Force say from {ToPrettyString(ent.Owner)} as {ToPrettyString(ent.Comp.PsychosisOwner)}: {arg}.");
            _chat.TrySendInGameICMessage(ent.Comp.PsychosisOwner, arg, Content.Shared.Chat.InGameICChatType.Speak, false, false);
        });
    }

    private void CreatePsychoEntity(EntityUid uid, CyberSanityComponent comp)
    {
        var ent = _shizophrenia.AddHallucination(uid, "HallucinationCyberPhychosis");
        _meta.SetEntityName(ent, Name(uid));
        _meta.SetEntityDescription(ent, Description(uid));

        var psyComp = EnsureComp<CyberPsychosisEntityComponent>(ent);
        psyComp.PsychosisOwner = uid;

        _humanoid.CloneAppearance(uid, ent);
        _humanoid.SetBaseLayerColor(ent, HumanoidVisualLayers.Eyes, Color.Crimson);

        // Clone speech
        if (TryComp<SpeechComponent>(uid, out var ourSpeech))
        {
            var speech = EnsureComp<SpeechComponent>(ent);
            speech.SpeechVerb = ourSpeech.SpeechVerb;
            speech.SpeechSounds = ourSpeech.SpeechSounds;
            speech.AllowedEmotes = ourSpeech.AllowedEmotes;
            speech.SuffixSpeechVerbs = ourSpeech.SuffixSpeechVerbs;
        }

        // Clone vocals
        if (TryComp<VocalComponent>(uid, out var ourVocal))
        {
            var vocal = EnsureComp<VocalComponent>(ent);
            vocal.EmoteSounds = ourVocal.EmoteSounds;
            vocal.Sounds = ourVocal.Sounds;
        }

        // Clone body emotes
        if (TryComp<BodyEmotesComponent>(uid, out var ourBodyEmotes))
        {
            var bodyEmotes = EnsureComp<BodyEmotesComponent>(ent);
            bodyEmotes.Sounds = ourBodyEmotes.Sounds;
        }

        // Clone inventory
        if (TryComp<InventoryComponent>(uid, out var inv))
        {
            var inventory = EnsureComp<InventoryComponent>(ent);
            _inventory.SetTemplateId((ent, inventory), inv.TemplateId);
            _inventory.SetDisplacements((ent, inventory), inv.Displacements, inv.FemaleDisplacements, inv.MaleDisplacements);

            var remove = EnsureComp<HallucinationRemoveInventoryComponentsComponent>(ent);

            foreach (var slot in inv.Slots)
            {
                if (_inventory.TryGetSlotEntity(uid, slot.Name, out var clothing) && TryPrototype(clothing.Value, out var proto))
                {
                    var item = Spawn(proto.ID);
                    _shizophrenia.AddAsHallucination(uid, item);

                    EnsureComp<UnremoveableComponent>(item);
                    EntityManager.RemoveComponents(item, remove.Components);

                    _inventory.TryEquip(ent, item, slot.Name, true, true, false);
                }
            }
        }

        EnsureComp<GhostTakeoverAvailableComponent>(ent);
    }
}
