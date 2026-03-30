using Content.Goobstation.Common.Traitor;
using Content.Goobstation.Common.Traitor.PenSpin;
using Content.Goobstation.Server.Traitor.PenSpin;
using Content.Goobstation.Shared.Traitor.PenSpin;
using Content.Server.PDA.Ringer;
using Content.Server.Preferences.Managers;
using Content.Server.Store.Systems;
using Content.Shared.Mind;
using Content.Shared.PDA;
using Content.Shared.PDA.Ringer;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Store;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Traitor;

public sealed class GoobUplinkSystem : GoobCommonUplinkSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly IServerPreferencesManager _prefs = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    private static readonly ProtoId<RoleLoadoutPrototype> AntagTraitorLoadout = "AntagTraitor";
    private static readonly ProtoId<LoadoutGroupPrototype> TraitorUplinkGroup = "TraitorUplink";
    private static readonly ProtoId<UplinkPreferencePrototype> DefaultPreference = "UplinkPda";

    private EntityQuery<ContainerManagerComponent> _containerQuery;

    public override void Initialize()
    {
        base.Initialize();

        _containerQuery = GetEntityQuery<ContainerManagerComponent>();

        SubscribeLocalEvent<PenSpinUplinkComponent, CurrencyInsertAttemptEvent>(OnCurrencyInsert);
        SubscribeLocalEvent<PenSpinUplinkComponent, BoundUIClosedEvent>(OnStoreClosed);
        SubscribeLocalEvent<PenComponent, PenSpinSubmitDegreeMessage>(OnSubmitDegree);
        SubscribeLocalEvent<PenComponent, PenSpinResetMessage>(OnReset);
        SubscribeLocalEvent<PenComponent, GenerateUplinkCodeEvent<int[]>>(OnGenerateCode);

        SubscribeLocalEvent<PdaComponent, SetupUplinkEvent>(OnSetupPdaUplink);
        SubscribeLocalEvent<PenComponent, SetupUplinkEvent>(OnSetupPenUplink);
    }

    private void OnSetupPdaUplink(Entity<PdaComponent> ent, ref SetupUplinkEvent ev)
    {
        EnsureComp<RingerUplinkComponent>(ent.Owner);

        var ringerEv = new GenerateUplinkCodeEvent<Note[]>();
        RaiseLocalEvent(ent.Owner, ref ringerEv);

        var code = Comp<RingerUplinkComponent>(ent.Owner).Code;
        if (code != null)
        {
            var codeStr = string.Join("-", code).Replace("sharp", "#");
            ev.BriefingEntry = Loc.GetString("traitor-role-uplink-code", ("code", codeStr));
            ev.BriefingEntryShort = Loc.GetString("traitor-role-uplink-code-short", ("code", codeStr));
        }

        ev.Handled = true;
    }

    private void OnSetupPenUplink(Entity<PenComponent> ent, ref SetupUplinkEvent ev)
    {
        EnsureComp<PenSpinUplinkComponent>(ent.Owner);

        var spinEv = new GenerateUplinkCodeEvent<int[]>();
        RaiseLocalEvent(ent.Owner, ref spinEv);

        if (spinEv.Code != null)
        {
            var codeStr = string.Join("-", spinEv.Code);
            ev.BriefingEntry = Loc.GetString("traitor-role-uplink-pen-code", ("code", codeStr));
            ev.BriefingEntryShort = Loc.GetString("traitor-role-uplink-pen-code-short", ("code", codeStr));
        }

        ev.Handled = true;
    }

    /// <remarks>Falls back to PDA if no preference is set.</remarks>
    public override ProtoId<UplinkPreferencePrototype> GetUplinkPreference(EntityUid mindEnt)
    {
        var mind = Comp<MindComponent>(mindEnt);

        if (mind.UserId == null)
            return DefaultPreference;

        var prefs = _prefs.GetPreferences(mind.UserId.Value);
        if (prefs.SelectedCharacter is not HumanoidCharacterProfile profile
            || !profile.Loadouts.TryGetValue(AntagTraitorLoadout, out var roleLoadout)
            || !roleLoadout.SelectedLoadouts.TryGetValue(TraitorUplinkGroup, out var selectedLoadouts))
            return DefaultPreference;

        foreach (var loadout in selectedLoadouts)
        {
            foreach (var uplinkPref in _proto.EnumeratePrototypes<UplinkPreferencePrototype>())
            {
                if (uplinkPref.Loadout != null && uplinkPref.Loadout == loadout.Prototype)
                    return uplinkPref.ID;
            }
        }

        return DefaultPreference;
    }

    // EntityWhitelistSystem.IsValid handles everything so string[] instead of compreg is fine here
    // deltanedas compreg ao3 fic when?
    public override EntityUid? FindUplinkTarget(EntityUid user, string[] searchComponents)
    {
        var whitelist = new EntityWhitelist { Components = searchComponents };

        if (!_containerQuery.TryGetComponent(user, out var currentManager))
            return null;

        var containerStack = new Stack<ContainerManagerComponent>();

        do
        {
            foreach (var container in currentManager.Containers.Values)
            {
                foreach (var entity in container.ContainedEntities)
                {
                    if (_whitelistSystem.IsValid(whitelist, entity))
                        return entity;

                    if (_containerQuery.TryGetComponent(entity, out var containerManager))
                        containerStack.Push(containerManager);
                }
            }
        } while (containerStack.TryPop(out currentManager));

        return null;
    }

    private void OnGenerateCode(Entity<PenComponent> ent, ref GenerateUplinkCodeEvent<int[]> ev)
    {
        var code = new int[ent.Comp.CombinationLength];
        for (var i = 0; i < ent.Comp.CombinationLength; i++)
        {
            code[i] = _random.Next(ent.Comp.MinDegree, ent.Comp.MaxDegree + 1);
        }

        if (TryComp<PenSpinUplinkComponent>(ent.Owner, out var uplink))
        {
            uplink.Code = code;
        }

        ev.Code = code;
    }

    private void OnCurrencyInsert(Entity<PenSpinUplinkComponent> ent, ref CurrencyInsertAttemptEvent args)
    {
        if (!ent.Comp.Unlocked)
            args.Cancel();
    }

    private void OnStoreClosed(Entity<PenSpinUplinkComponent> ent, ref BoundUIClosedEvent args)
    {
        if (args.UiKey is StoreUiKey)
            ent.Comp.Unlocked = false;
    }

    private void OnSubmitDegree(Entity<PenComponent> ent, ref PenSpinSubmitDegreeMessage args)
    {
        if (!IsValidDegree(ent.Comp, args.Degree))
            return;

        if (!TryComp<PenSpinUplinkComponent>(ent, out var uplink))
            return;

        var curTime = _timing.CurTime;
        if (uplink.NextSpinTime.HasValue && curTime < uplink.NextSpinTime.Value)
            return;

        uplink.NextSpinTime = curTime + ent.Comp.SpinCooldown;

        if (uplink.CurrentCombination.Length != ent.Comp.CombinationLength)
            uplink.CurrentCombination = new int[ent.Comp.CombinationLength];

        uplink.CurrentCombination[uplink.CurrentIndex] = args.Degree;

        var hasCode = uplink.Code is not null;
        var isCorrect = hasCode && args.Degree == uplink.Code![uplink.CurrentIndex];

        if (isCorrect)
        {
            uplink.CurrentIndex++;

            if (uplink.CurrentIndex >= ent.Comp.CombinationLength)
            {
                uplink.Unlocked = true;
                _ui.OpenUi(ent.Owner, StoreUiKey.Key, args.Actor);
                ResetCombination(ent.Comp, uplink);
            }
        }
        else
        {
            ResetCombination(ent.Comp, uplink);
        }
    }

    private void OnReset(Entity<PenComponent> ent, ref PenSpinResetMessage args)
    {
        if (TryComp<PenSpinUplinkComponent>(ent, out var uplink))
            ResetCombination(ent.Comp, uplink);
    }

    private static void ResetCombination(PenComponent spin, PenSpinUplinkComponent uplink)
    {
        uplink.CurrentCombination = new int[spin.CombinationLength];
        uplink.CurrentIndex = 0;
    }

    private static bool IsValidDegree(PenComponent comp, int degree)
    {
        return degree >= comp.MinDegree && degree <= comp.MaxDegree;
    }
}
