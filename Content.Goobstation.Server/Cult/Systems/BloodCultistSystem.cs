using Content.Goobstation.Server.Cult.GameTicking;
using Content.Goobstation.Shared.Cult;
using Content.Goobstation.Shared.Cult.Magic;
using Content.Server._EinsteinEngines.Language;
using Content.Server.Antag;

namespace Content.Goobstation.Server.Cult.Systems;

// todo add chat interaction
public sealed partial class BloodCultistSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly LanguageSystem _lang = default!;
    [Dependency] private readonly BloodCultRuleSystem _bloodCultRule = default!;
    [Dependency] private readonly BloodMagicSystem _bloodMagic = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultistComponent, ComponentStartup>(OnCultistComponentStartup);
        SubscribeLocalEvent<BloodCultistComponent, ComponentShutdown>(OnCultistComponentShutdown);

        SubscribeLocalEvent<BloodCultistLeaderComponent, ComponentInit>(OnLeaderComponentInit);
        SubscribeLocalEvent<BloodCultistLeaderComponent, ComponentShutdown>(OnLeaderComponentShutdown);
    }

    private void OnCultistComponentStartup(Entity<BloodCultistComponent> ent, ref ComponentStartup args)
    {
        if (!_bloodCultRule.TryGetRule(out var rule))
            return;

        var gamerule = rule!.Value;

        gamerule.Comp.Cultists.Add(ent);
    }

    private void OnCultistComponentShutdown(Entity<BloodCultistComponent> ent, ref ComponentShutdown args)
    {
        _antag.SendBriefing(ent, Loc.GetString("cult-loss-self"), Color.Crimson, BloodCultRuleSystem.UnimportantAnnouncementSound);
        _lang.RemoveLanguage(ent.Owner, BloodCultRuleSystem.CultLanguage);
        RemComp<BloodMagicProviderComponent>(ent);
    }

    private void OnLeaderComponentInit(Entity<BloodCultistLeaderComponent> ent, ref ComponentInit args)
    {
        _antag.SendBriefing(ent, Loc.GetString("cult-master-gain-briefing"), Color.Crimson, BloodCultRuleSystem.UnimportantAnnouncementSound);

        if (TryComp<BloodMagicProviderComponent>(ent, out var bmp))
            foreach (var spell in bmp.LeaderSpells)
                _bloodMagic.TryGrantSpell((ent, bmp), spell, false);
    }

    private void OnLeaderComponentShutdown(Entity<BloodCultistLeaderComponent> ent, ref ComponentShutdown args)
    {
        _antag.SendBriefing(ent, Loc.GetString("cult-master-loss-self"), Color.Crimson, BloodCultRuleSystem.UnimportantAnnouncementSound);

        if (_bloodCultRule.TryGetRule(out var rule) && rule!.Value.Comp.CultLeader == ent)
        {
            var gamerule = rule!.Value;
            gamerule.Comp.CultLeader = null;
            _bloodCultRule.DoCultAnnouncement(gamerule, Loc.GetString("cult-master-loss"));
            _bloodCultRule.ScheduleLeaderElection(gamerule);
        }

        if (TryComp<BloodMagicProviderComponent>(ent, out var bmp))
            foreach (var spell in bmp.LeaderSpells)
                _bloodMagic.TryRemoveSpell((ent, bmp), spell);
    }
}
