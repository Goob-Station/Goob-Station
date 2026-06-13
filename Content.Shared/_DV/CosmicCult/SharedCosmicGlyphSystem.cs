using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Examine;
using Robust.Shared.Timing;

namespace Content.Shared._DV.CosmicCult;

public abstract class SharedCosmicGlyphSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedCosmicCultSystem _cosmicCult = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CosmicGlyphComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<CosmicGlyphComponent> uid, ref ExaminedEvent args)
    {
        if (_cosmicCult.EntityIsCultist(args.Examiner))
        {
            args.PushMarkup(Loc.GetString("cosmic-examine-glyph-cultcount", ("COUNT", uid.Comp.RequiredCultists)));
        }
    }

    public void EraseGlyph(EntityUid ent)
    {
        if (!TryComp<CosmicGlyphComponent>(ent, out var comp)) return;
        _appearance.SetData(ent, GlyphVisuals.Status, GlyphStatus.Despawning);
        comp.State = GlyphStatus.Despawning;
        comp.Timer = _timing.CurTime + comp.DespawnTime;
    }
}
