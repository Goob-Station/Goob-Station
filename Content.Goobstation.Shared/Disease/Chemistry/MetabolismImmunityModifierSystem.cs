using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Disease.Chemistry
{
    public sealed class MetabolismImmunityModifierSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;

        private readonly List<Entity<ImmunityModifierMetabolismComponent>> _components = new();

        public override void Initialize()
        {
            base.Initialize();

            UpdatesOutsidePrediction = true;

            SubscribeLocalEvent<ImmunityModifierMetabolismComponent, ComponentStartup>(AddComponent);
            SubscribeLocalEvent<ImmunityModifierMetabolismComponent, GetImmunityEvent>(OnGetImmunity);
        }

        private void OnGetImmunity(EntityUid uid, ImmunityModifierMetabolismComponent component, GetImmunityEvent args)
        {
            args.ImmunityGainRate += component.GainRateModifier;
            args.ImmunityStrength += component.StrengthModifier;
        }

        private void AddComponent(Entity<ImmunityModifierMetabolismComponent> metabolism, ref ComponentStartup args)
        {
            _components.Add(metabolism);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var currentTime = _gameTiming.CurTime;

            for (var i = _components.Count - 1; i >= 0; i--)
            {
                var metabolism = _components[i];

                if (metabolism.Comp.Deleted)
                {
                    _components.RemoveAt(i);
                    continue;
                }

                if (metabolism.Comp.ModifierTimer > currentTime)
                    continue;

                _components.RemoveAt(i);
                RemComp<ImmunityModifierMetabolismComponent>(metabolism);
            }
        }
    }
}
