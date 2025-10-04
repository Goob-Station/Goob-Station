using Content.Goobstation.Common.Medical;
using Content.Goobstation.Server.Shizophrenia;
using Content.Goobstation.Shared.Shizophrenia;
using Content.Server.Body.Systems;
using Content.Server.Emoting.Components;
using Content.Server.Ghost.Roles.Components;
using Content.Shared.Actions.Components;
using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;

namespace Content.Goobstation.Server.Cyberpsychosis;

public sealed partial class CyberSanitySystem
{
    private void CreatePsychoEntity(EntityUid uid, CyberSanityComponent comp)
    {
        var ent = _shizophrenia.AddHallucination(uid, "HallucinationCyberPhychosis");

        _humanoid.CloneAppearance(uid, ent);

        if (TryComp<SpeechComponent>(uid, out var ourSpeech))
        {
            var speech = EnsureComp<SpeechComponent>(ent);
            speech.SpeechVerb = ourSpeech.SpeechVerb;
            speech.SpeechSounds = ourSpeech.SpeechSounds;
            speech.AllowedEmotes = ourSpeech.AllowedEmotes;
            speech.SuffixSpeechVerbs = ourSpeech.SuffixSpeechVerbs;
        }

        if (TryComp<VocalComponent>(uid, out var ourVocal))
        {
            var vocal = EnsureComp<VocalComponent>(ent);
            vocal.EmoteSounds = ourVocal.EmoteSounds;
            vocal.Sounds = ourVocal.Sounds;
        }

        if (TryComp<BodyEmotesComponent>(uid, out var ourBodyEmotes))
        {
            var bodyEmotes = EnsureComp<BodyEmotesComponent>(ent);
            bodyEmotes.Sounds = ourBodyEmotes.Sounds;
        }

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
