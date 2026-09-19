using Content.Goobstation.Common.Wizard.Events;
using Content.Server._Goobstation.Wizard.Systems;
using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Managers;
using Content.Shared.Atmos;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Parallax;
using Robust.Server.Audio;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Wizard.Systems;

public sealed partial class DimensionShiftSystem : EntitySystem
{
    [Dependency] private readonly WizardRuleSystem _wizardRule = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DimensionShiftEvent>(OnDimensionShift);
    }

    private void OnDimensionShift(DimensionShiftEvent ev)
    {
        var map = _wizardRule.GetTargetMap();
        if (map == null)
            return;

        if (ev.Parallax != null)
        {
            var parallax = EnsureComp<ParallaxComponent>(map.Value);
            parallax.Parallax = ev.Parallax;
            Dirty(map.Value, parallax);
        }

        var moles = new float[Atmospherics.AdjustedNumberOfGases];
        moles[(int) Gas.Oxygen] = ev.OxygenMoles;
        moles[(int) Gas.Nitrogen] = ev.NitrogenMoles;
        moles[(int) Gas.CarbonDioxide] = ev.CarbonDioxideMoles;

        var mixture = new GasMixture(moles, ev.Temperature);

        _atmos.SetMapAtmosphere(map.Value, false, mixture);

        var message = Loc.GetString("dimension-shift-message");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
        _chatManager.ChatMessageToAll(ChatChannel.Radio, message, wrappedMessage, default, false, true, Color.Red);
        _audio.PlayGlobal(ev.Sound, Filter.Broadcast(), true);

        _adminLogManager.Add(LogType.EventRan, LogImpact.Extreme, $"Station map changed via wizard spellbook dimension shift.");

        return;
    }
}