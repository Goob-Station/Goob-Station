using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Utility;

namespace Content.Pirate.Shared.Showers
{
    public abstract class SharedShowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<ShowerComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<ShowerComponent, GetVerbsEvent<AlternativeVerb>>(OnToggleShowerVerb);
            SubscribeLocalEvent<ShowerComponent, ActivateInWorldEvent>(OnActivateInWorld);
            SubscribeLocalEvent<ShowerComponent, ExaminedEvent>(OnExamined);
        }

        private void OnExamined(Entity<ShowerComponent> ent, ref ExaminedEvent args)
        {
            if (!args.IsInDetailsRange ||
                !_solution.TryGetSolution(ent.Owner, ent.Comp.SolutionName, out _, out var solution))
            {
                return;
            }

            args.PushMarkup(Loc.GetString("shower-examine-water",
                ("current", solution.Volume),
                ("max", solution.MaxVolume)));
        }

        private void OnMapInit(EntityUid uid, ShowerComponent component, MapInitEvent args)
        {
            component.ToggleShower = false;
            UpdateAppearance(uid, component);
        }

        private void OnToggleShowerVerb(EntityUid uid, ShowerComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess || args.Hands == null)
                return;

            var user = args.User;
            AlternativeVerb toggleVerb = new()
            {
                Act = () => ToggleShowerHead(uid, user, component)
            };

            if (!component.ToggleShower)
            {
                toggleVerb.Text = Loc.GetString("shower-turn-on");
                toggleVerb.Icon =
                    new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/open.svg.192dpi.png"));
            }
            else
            {
                toggleVerb.Text = Loc.GetString("shower-turn-off");
                toggleVerb.Icon =
                    new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/close.svg.192dpi.png"));
            }
            args.Verbs.Add(toggleVerb);
        }

        private void OnActivateInWorld(EntityUid uid, ShowerComponent comp, ActivateInWorldEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;
            ToggleShowerHead(uid, args.User, comp);
        }

        public void ToggleShowerHead(EntityUid uid, EntityUid? user = null, ShowerComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            var newState = !component.ToggleShower;

            // Dry showers must refill before use.
            if (newState && IsDry(uid, component))
            {
                if (user != null)
                    _popup.PopupClient(Loc.GetString("shower-dry"), uid, user.Value);
                return;
            }

            SetShower(uid, newState, component);

            if (newState)
                _audio.PlayPvs(component.EnableShowerSound, uid);
        }

        /// <summary>Forces the shower on or off.</summary>
        public void SetShower(EntityUid uid, bool on, ShowerComponent? component = null)
        {
            if (!Resolve(uid, ref component) || component.ToggleShower == on)
                return;

            component.ToggleShower = on;
            Dirty(uid, component);
            UpdateAppearance(uid, component);
        }

        /// <summary>True when the tank holds less than one spray dose.</summary>
        private bool IsDry(EntityUid uid, ShowerComponent component)
        {
            // Missing client-side solution data should not block interaction.
            return _solution.TryGetSolution(uid, component.SolutionName, out _, out var solution)
                   && solution.Volume < component.SprayAmount;
        }

        private void UpdateAppearance(EntityUid uid, ShowerComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            _appearance.SetData(uid, ShowerVisuals.ShowerVisualState,
                component.ToggleShower ? ShowerVisualState.On : ShowerVisualState.Off);

            if (component.ToggleShower)
            {
                if (component.PlayingStream == null)
                {
                    component.PlayingStream = _audio.PlayPvs(
                        component.LoopingSound,
                        uid,
                        AudioParams.Default.WithLoop(true).WithMaxDistance(5)
                    )?.Entity;
                }
            }
            else
            {
                component.PlayingStream = _audio.Stop(component.PlayingStream);
                component.PlayingStream = null;
            }
        }
    }
}
