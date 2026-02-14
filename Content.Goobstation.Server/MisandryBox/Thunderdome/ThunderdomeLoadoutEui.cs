using Content.Goobstation.Shared.MisandryBox.Thunderdome;
using Content.Server.EUI;
using Content.Shared.Eui;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.MisandryBox.Thunderdome;

public sealed class ThunderdomeLoadoutEui : BaseEui
{
    private readonly ThunderdomeRuleSystem _thunderdomeSystem;
    private readonly EntityUid _ruleEntity;
    private readonly ICommonSession _session;

    public ThunderdomeLoadoutEui(ThunderdomeRuleSystem thunderdomeSystem, EntityUid ruleEntity, ICommonSession session)
    {
        _thunderdomeSystem = thunderdomeSystem;
        _ruleEntity = ruleEntity;
        _session = session;
    }

    public override EuiStateBase GetNewState()
    {
        var entManager = IoCManager.Resolve<IEntityManager>();
        if (!entManager.TryGetComponent<ThunderdomeRuleComponent>(_ruleEntity, out var rule))
            return new ThunderdomeLoadoutEuiState(new List<ThunderdomeLoadoutOption>(), 0);

        return _thunderdomeSystem.GetLoadoutState(rule);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        switch (msg)
        {
            case ThunderdomeLoadoutSelectedMessage selected:
                _thunderdomeSystem.SpawnPlayer(_session, _ruleEntity, selected.WeaponIndex);
                Close();
                break;
        }
    }

    public override void Opened()
    {
        base.Opened();
        StateDirty();
    }

    public override void Closed()
    {
        base.Closed();
    }
}
