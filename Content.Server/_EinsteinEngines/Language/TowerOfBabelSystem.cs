using System.Linq;
using Content.Shared._EinsteinEngines.Language;
using Content.Shared._EinsteinEngines.Language.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._EinsteinEngines.Language;

public sealed class TowerOfBabelSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TowerOfBabelComponent, MapInitEvent>(OnInit, before: [typeof(LanguageSystem)]);
    }

    private void OnInit(Entity<TowerOfBabelComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp(ent, out LanguageKnowledgeComponent? knowledge) ||
            !TryComp(ent, out LanguageSpeakerComponent? speaker))
            return;

        if (!_prototype.TryGetInstances<LanguagePrototype>(out var langs))
            return;

        knowledge.SpokenLanguages = langs.Keys.Select(x => new ProtoId<LanguagePrototype>(x)).ToList();
        knowledge.UnderstoodLanguages = knowledge.SpokenLanguages.ToList();
        speaker.CurrentLanguage = ent.Comp.DefaultLanguage;
    }
}
