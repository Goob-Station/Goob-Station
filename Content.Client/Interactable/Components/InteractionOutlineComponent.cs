using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client.Interactable.Components
{
    [RegisterComponent]
    public sealed partial class InteractionOutlineComponent : Component
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IEntityManager _entMan = default!;

        private const float DefaultWidth = 1;

        [ValidatePrototypeId<ShaderPrototype>]
        private const string ShaderInRange = "SelectionOutlineInrange";

        [ValidatePrototypeId<ShaderPrototype>]
        private const string ShaderOutOfRange = "SelectionOutline";

        private bool _inRange;
        private ShaderInstance? _shader;
        private int _lastRenderScale;

        private static ShaderInstance? _shaderInRange;
        private static ShaderInstance? _shaderOutOfRange;


        public void OnMouseEnter(EntityUid uid, bool inInteractionRange, int renderScale)
        {
            _lastRenderScale = renderScale;
            _inRange = inInteractionRange;
            if (_entMan.TryGetComponent(uid, out SpriteComponent? sprite) && sprite.PostShader == null)
            {
                var shader = GetOrCreateShader(inInteractionRange, renderScale);
                sprite.PostShader = shader;
            }
        }

        public void OnMouseLeave(EntityUid uid)
        {
            if (_entMan.TryGetComponent(uid, out SpriteComponent? sprite))
            {
                if (sprite.PostShader == (_inRange ? _shaderInRange : _shaderOutOfRange))
                    sprite.PostShader = null;
                sprite.RenderOrder = 0;
            }
        }

        public void UpdateInRange(EntityUid uid, bool inInteractionRange, int renderScale)
        {
            if (_entMan.TryGetComponent(uid, out SpriteComponent? sprite)
                && (inInteractionRange != _inRange || _lastRenderScale != renderScale))
            {
                _inRange = inInteractionRange;
                _lastRenderScale = renderScale;

                var shader = GetOrCreateShader(_inRange, _lastRenderScale);
                sprite.PostShader = shader;
            }
        }

        //RESERVE STATION
        private ShaderInstance GetOrCreateShader(bool inRange, int renderScale)
        {
            if (inRange)
            {
                if (_shaderInRange == null)
                {
                    _shaderInRange = _prototypeManager.Index<ShaderPrototype>(ShaderInRange).InstanceUnique();
                }
                _shaderInRange.SetParameter("outline_width", DefaultWidth * renderScale);
                return _shaderInRange;
            }
            else
            {
                if (_shaderOutOfRange == null)
                {
                    _shaderOutOfRange = _prototypeManager.Index<ShaderPrototype>(ShaderOutOfRange).InstanceUnique();
                }
                _shaderOutOfRange.SetParameter("outline_width", DefaultWidth * renderScale);
                return _shaderOutOfRange;
            }
        }
    }
}
