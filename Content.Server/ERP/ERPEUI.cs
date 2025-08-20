using Content.Server.EUI;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Content.Shared.ERP;
using Content.Shared.Humanoid;
using Robust.Server.GameObjects;
namespace Content.Server.ERP
{
    [UsedImplicitly]
    public sealed class ERPEUI : BaseEui
    {
        private readonly NetEntity _target;
        private readonly Sex _userSex;
        private readonly Sex _targetSex;
        private readonly bool _userHasClothing;
        private readonly bool _targetHasClothing;
        private readonly bool _erpAllowed;


        private readonly ERPSystem _interaction;
        private readonly TransformSystem _transform;
        public IEntityManager _entManager;
        public ERPEUI(NetEntity target, Sex userSex, bool userHasClothing, Sex targetSex, bool targetHasClothing, bool erpAllowed)
        {
            _target = target;
            _userSex = userSex;
            _userHasClothing = userHasClothing;
            _targetSex = targetSex;
            _targetHasClothing = targetHasClothing;
            _erpAllowed = erpAllowed;
            _interaction = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ERPSystem>();
            _transform = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<TransformSystem>();
            _entManager = IoCManager.Resolve<IEntityManager>();
            IoCManager.InjectDependencies(this);
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);

            switch (msg)
            {
                case AddLoveMessage req:
                    _interaction.AddLove(req.User, req.Target, req.Percent);
                    if(_entManager.TryGetComponent<ERPComponent>(_entManager.GetEntity(req.User), out var usComp))
                    {
                        SendMessage(new ResponseLoveMessage(usComp.Love));
                    }
                    if(!_transform.InRange(_transform.GetMoverCoordinates(_entManager.GetEntity(req.User)), _transform.GetMoverCoordinates(_entManager.GetEntity(req.Target)), 2))
                    {
                        Close();
                    }
                    break;
            }
        }


        public override void Opened()
        {
            base.Opened();

            StateDirty();
        }

        public override EuiStateBase GetNewState()
        {
            return new SetInteractionEuiState
            {
                TargetNetEntity = _target,
                UserSex = _userSex,
                UserHasClothing = _userHasClothing,
                TargetSex = _targetSex,
                TargetHasClothing = _targetHasClothing,
                ErpAllowed = _erpAllowed,
            };
        }

    }
}
