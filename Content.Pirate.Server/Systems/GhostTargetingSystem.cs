using System;
using System.Linq;
using System.Collections.Generic;
using Robust.Shared.Audio.Systems;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Robust.Shared.Map;
using Robust.Shared.Audio;
using Robust.Shared.Utility;
using Robust.Shared.Physics.Dynamics;

using Robust.Shared.Log;
using Robust.Shared.GameObjects;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

using Content.Shared.Movement;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Ghost;
using Content.Shared.Actions;
using Content.Shared._Shitmed.Body.Components;
using Content.Shared._RMC14.GhostColor;
using Content.Shared.Popups;
using Content.Shared.Shuttles.Components;
using Content.Shared.Examine;
using Content.Shared.Speech;

using Content.Server.Zombies;
using Content.Server.Atmos.Components;
using Content.Server.Speech.Components;
using Content.Server.Actions;
using Content.Server._EinsteinEngines.Language;

using Content.Goobstation.Shared.Religion;
using Content.Goobstation.Shared.CrematorImmune;

using Content.Pirate.Shared.Components;
using Content.Pirate.Shared;

namespace Content.Pirate.Server.Systems
{
    public sealed class GhostTargetingSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<GhostTargetingComponent, MapInitEvent>(OnStartup);
            SubscribeLocalEvent<GhostTargetingComponent, ComponentRemove>(OnRemove);
            SubscribeLocalEvent<GhostTargetingComponent, ClearTargetGhostActionEvent>(OnClearTargetGhost);
            SubscribeLocalEvent<GhostTargetingComponent, SetTargetGhostActionEvent>(OnSetTargetGhost);
            SubscribeLocalEvent<GhostTargetingComponent, ToggleGhostFormActionEvent>(OnToggleGhostForm);
            SubscribeLocalEvent<GhostTargetingComponent, GhostBlinkActionEvent>(OnGhostBlinkAction);
        }
        private void OnGhostBlinkAction(EntityUid uid, GhostTargetingComponent comp, GhostBlinkActionEvent args)
        {
            var entityManager = EntityManager;
            if (!comp.IsGhost)
                return;
            if (!entityManager.TryGetComponent(uid, out TransformComponent? xform) || xform == null)
                return;
            var direction = xform.LocalRotation.ToWorldVec();
            var distance = 4f;
            var oldPos = xform.Coordinates;
            var newPos = oldPos.Offset(direction * distance);

            var effectProto = "SwapSpellEffect";
            var soundProto = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/swap.ogg");
            var audioSys = entityManager.System<SharedAudioSystem>();

            void SpawnSwapEffect(EntityCoordinates coords)
            {
                audioSys.PlayPvs(soundProto, coords);
                var effect = entityManager.SpawnEntity(effectProto, coords);

                // Ensure VisibilityComponent exists
                var visComp = entityManager.EnsureComponent<VisibilityComponent>(effect);
                var visSys = entityManager.System<VisibilitySystem>();
                visSys.AddLayer((effect, visComp), (int)Content.Shared.Eye.VisibilityFlags.TargetingGhost, false);
                visSys.RefreshVisibility(effect, visibilityComponent: visComp);

                if (entityManager.TryGetComponent<TrailComponent>(effect, out var trail))
                {
                    var transformSystem = entityManager.System<SharedTransformSystem>();
                    trail.SpawnPosition = coords.ToMap(entityManager, transformSystem).Position;
                    trail.RenderedEntity = uid;
                    Dirty(effect, trail);
                }
            }

            SpawnSwapEffect(oldPos);
            SpawnSwapEffect(newPos);

            xform.Coordinates = newPos;
        }
        private void OnStartup(Entity<GhostTargetingComponent> ghost, ref MapInitEvent args)
        {
            // Додаємо базові action-прототипи
            if (ghost.Comp.ActionEntities == null)
                ghost.Comp.ActionEntities = new List<EntityUid>();

            foreach (var actionId in ghost.Comp.BaseGhostActions)
            {
                var ent = EntityManager.System<ActionsSystem>().AddAction(ghost, actionId);
                if (ent != null)
                {
                    ghost.Comp.ActionEntities.Add(ent.Value);
                    if (actionId == "ActionToggleGhostForm")
                        ghost.Comp.ToggleGhostFormActionUid = ent.Value;
                }
            }

            foreach (var ent in ghost.Comp.ActionEntities)
            {
                var comps = EntityManager.GetComponents(ent);
                var compNames = string.Join(", ", comps.Select(c => c.GetType().Name));
            }

            // EyeComponent: якщо вже є ціль, одразу оновити маску видимості
            var ghostLayer = (int)Content.Shared.Eye.VisibilityFlags.TargetingGhost;
            if (ghost.Comp.Target != NetEntity.Invalid)
            {
                var targetUid = EntityManager.GetEntity(ghost.Comp.Target);
                if (EntityManager.TryGetComponent<EyeComponent>(targetUid, out var targetEye) && targetEye != null)
                {
                    var eyeSys = EntityManager.System<EyeSystem>();
                    eyeSys.SetVisibilityMask(targetUid, targetEye.VisibilityMask | ghostLayer, targetEye);
                }
            }
        }

        private void OnRemove(Entity<GhostTargetingComponent> ghost, ref ComponentRemove args)
        {
            // Відновлюємо стару форму, якщо entity був у ghost-формі
            if (ghost.Comp.OldVisibilityLayers.HasValue)
            {
                var uid = ghost.Owner;
                var visSys = EntityManager.System<VisibilitySystem>();

                // Eye
                if (EntityManager.TryGetComponent<EyeComponent>(uid, out var eyeNormal) && eyeNormal != null)
                {
                    var eyeSys = EntityManager.System<EyeSystem>();
                    // Прибираємо TargetingGhost layer з EyeComponent
                    var mask = eyeNormal.VisibilityMask & ~(int)Content.Shared.Eye.VisibilityFlags.TargetingGhost;
                    if (ghost.Comp.OldVisibilityMask.HasValue)
                        mask = ghost.Comp.OldVisibilityMask.Value & ~(int)Content.Shared.Eye.VisibilityFlags.TargetingGhost;
                    eyeSys.SetVisibilityMask(uid, mask, eyeNormal);
                    if (ghost.Comp.OldDrawFov.HasValue)
                        eyeSys.SetDrawFov(uid, ghost.Comp.OldDrawFov.Value, eyeNormal);
                }

                // Physics
                if (EntityManager.TryGetComponent(uid, out PhysicsComponent? physics) && physics != null)
                {
                    var physicsSys = EntityManager.System<SharedPhysicsSystem>();
                    if (ghost.Comp.OldCanCollide.HasValue)
                        physicsSys.SetCanCollide(uid, ghost.Comp.OldCanCollide.Value, body: physics);
                    if (ghost.Comp.OldBodyType.HasValue)
                        physicsSys.SetBodyType(uid, ghost.Comp.OldBodyType.Value, body: physics);
                }

                // Відновити старі шари видимості
                if (EntityManager.TryGetComponent(uid, out VisibilityComponent? visComp2) && visComp2 != null)
                {
                    // Видаляємо TargetingGhost і Normal
                    visSys.RemoveLayer((uid, visComp2), (int)Content.Shared.Eye.VisibilityFlags.TargetingGhost, false);
                    visSys.RemoveLayer((uid, visComp2), (int)Content.Shared.Eye.VisibilityFlags.Normal, false);

                    // Додаємо всі біти зі старого Layer
                    ushort layers = (ushort)ghost.Comp.OldVisibilityLayers.Value;
                    for (int i = 0; i < 16; i++)
                    {
                        ushort flag = (ushort)(1 << i);
                        if ((layers & flag) != 0)
                            visSys.AddLayer((uid, visComp2), flag, false);
                    }
                    visSys.RefreshVisibility(uid, visibilityComponent: visComp2);
                }

                // Видалити компоненти, якщо їх не було до перетворення
                if (!ghost.Comp.HadContentEye && EntityManager.HasComponent<ContentEyeComponent>(uid))
                    EntityManager.RemoveComponent<ContentEyeComponent>(uid);
                if (!ghost.Comp.HadMovementIgnoreGravity && EntityManager.HasComponent<MovementIgnoreGravityComponent>(uid))
                    EntityManager.RemoveComponent<MovementIgnoreGravityComponent>(uid);
                if (!ghost.Comp.HadCanMoveInAir && EntityManager.HasComponent<CanMoveInAirComponent>(uid))
                    EntityManager.RemoveComponent<CanMoveInAirComponent>(uid);
                if (!ghost.Comp.HadPhysics && EntityManager.HasComponent<PhysicsComponent>(uid))
                    EntityManager.RemoveComponent<PhysicsComponent>(uid);
            }

            // Видаляємо всі action entity, якщо вони були додані
            if (ghost.Comp.ActionEntities != null)
            {
                foreach (var ent in ghost.Comp.ActionEntities)
                {
                    EntityManager.System<ActionsSystem>().RemoveAction(ghost, ent);
                }
                ghost.Comp.ActionEntities.Clear();
            }
        }

        public void OnClearTargetGhost(EntityUid uid, GhostTargetingComponent comp, ClearTargetGhostActionEvent args)
        {
            var ghostLayer = (int)Content.Shared.Eye.VisibilityFlags.TargetingGhost;
            var popupSys = EntityManager.System<SharedPopupSystem>();

            // Якщо була ціль, видаляємо маску з її ока
            if (comp.Target != NetEntity.Invalid)
            {
                var targetUid = EntityManager.GetEntity(comp.Target);
                if (EntityManager.TryGetComponent<EyeComponent>(targetUid, out var eye))
                {
                    var eyeSys = EntityManager.System<EyeSystem>();
                    eyeSys.SetVisibilityMask(targetUid, eye.VisibilityMask & ~ghostLayer, eye);
                }
                // Popup для користувача з ім'ям цілі
                var name = EntityManager.GetComponent<MetaDataComponent>(targetUid).EntityName;
                popupSys.PopupEntity($"Ціль '{name}' скинута!", uid, PopupType.Medium);
            }

            // Очищаємо ціль
            comp.Target = NetEntity.Invalid;
        }
        public void OnSetTargetGhost(EntityUid uid, GhostTargetingComponent comp, SetTargetGhostActionEvent args)
        {
            var ghostLayer = (int)Content.Shared.Eye.VisibilityFlags.TargetingGhost;
            var popupSys = EntityManager.System<SharedPopupSystem>();

            // Якщо була стара ціль, видаляємо маску з її ока
            if (comp.Target != NetEntity.Invalid)
            {
                var oldTargetUid = EntityManager.GetEntity(comp.Target);
                if (EntityManager.TryGetComponent<EyeComponent>(oldTargetUid, out var oldEye))
                {
                    var eyeSys = EntityManager.System<EyeSystem>();
                    eyeSys.SetVisibilityMask(oldTargetUid, oldEye.VisibilityMask & ~ghostLayer, oldEye);
                }
                // Додатковий popup про очищення старої цілі
                if (EntityManager.TryGetComponent<MetaDataComponent>(oldTargetUid, out var meta))
                {
                    var oldName = meta.EntityName;
                    popupSys.PopupEntity($"Стара ціль '{oldName}' очищена!", uid, PopupType.Small);
                }
            }

            // Встановлюємо нову ціль і даємо їй бачити привида
            if (args.Target != EntityUid.Invalid)
            {
                comp.Target = EntityManager.GetNetEntity(args.Target);

                // Додаємо маску новій цілі
                if (EntityManager.TryGetComponent(args.Target, out EyeComponent? newEye))
                {
                    var eyeSys = EntityManager.System<EyeSystem>();
                    eyeSys.SetVisibilityMask(args.Target, newEye.VisibilityMask | ghostLayer, newEye);
                }

                // Popup для користувача з ім'ям цілі
                var name = EntityManager.GetComponent<MetaDataComponent>(args.Target).EntityName;
                popupSys.PopupEntity($"Ціль встановлено: '{name}'!", uid, PopupType.Medium);
            }
        }


        public void OnToggleGhostForm(EntityUid uid, GhostTargetingComponent comp, ToggleGhostFormActionEvent args)
        {
            var popupSys = EntityManager.System<SharedPopupSystem>();
            var actionEntity = comp.ToggleGhostFormActionUid ?? args.Action;
            if (!comp.IsGhost)
            {
                SaveState(uid, comp);
                SaveImmunities(uid, comp);
                AddGhostComponents(uid, comp);
                AddImmunities(uid, comp);
                SetupEyePhysicsVisibility(uid, comp, ghostMode: true);
                comp.IsGhost = true;

                if (EntityManager.TryGetComponent<InstantActionComponent>(actionEntity, out var actionComp))
                {
                    actionComp.Icon = new Robust.Shared.Utility.SpriteSpecifier.Texture(new ResPath("Interface/Actions/eyeclose.png"));
                }
                popupSys.PopupEntity("Ви перетворилися на привида!", uid, PopupType.Medium);
            }
            else
            {
                RestoreState(uid, comp);
                RestoreImmunities(uid, comp);
                RemoveGhostComponents(uid, comp);
                RemoveImmunities(uid, comp);
                SetupEyePhysicsVisibility(uid, comp, ghostMode: false);
                ClearSavedState(comp);
                comp.IsGhost = false;

                if (EntityManager.TryGetComponent<InstantActionComponent>(actionEntity, out var actionComp))
                {
                    actionComp.Icon = new Robust.Shared.Utility.SpriteSpecifier.Texture(new ResPath("Interface/Actions/eyeopen.png"));
                }
                popupSys.PopupEntity("Ви повернули свою стару форму!", uid, PopupType.Medium);
            }
        }

        //  Приватні методи для логіки
        private void SaveState(EntityUid uid, GhostTargetingComponent comp)
        {
            // Додаткові компоненти
            comp.HadFTLSmashImmune = EntityManager.HasComponent<FTLSmashImmuneComponent>(uid);
            comp.HadUniversalLanguageSpeaker = EntityManager.HasComponent<UniversalLanguageSpeakerComponent>(uid);
            comp.HadExaminer = EntityManager.HasComponent<ExaminerComponent>(uid);
            comp.HadSpeechDead = false;
            if (EntityManager.TryGetComponent<SpeechComponent>(uid, out var speech) && speech != null)
            {
                comp.HadSpeechDead = speech.SpeechVerb == "Dead";
                comp.OldSpeechVerb = speech.SpeechVerb;
            }

            var visComp = EntityManager.EnsureComponent<VisibilityComponent>(uid);
            comp.OldVisibilityLayers = visComp.Layer;
            if (EntityManager.TryGetComponent<EyeComponent>(uid, out var eyeGhost) && eyeGhost != null)
            {
                comp.OldVisibilityMask = eyeGhost.VisibilityMask;
                comp.OldDrawFov = eyeGhost.DrawFov;
            }
            if (EntityManager.TryGetComponent(uid, out PhysicsComponent? physics) && physics != null)
            {
                comp.OldCanCollide = physics.CanCollide;
                comp.OldBodyType = physics.BodyType;
                comp.HadPhysics = true;
            }
            else
            {
                comp.HadPhysics = false;
            }

            comp.OldFixtureLayers = new List<int>();
            if (EntityManager.TryGetComponent(uid, out FixturesComponent? fixtures) && fixtures != null)
            {
                foreach (var fixture in fixtures.Fixtures.Values)
                {
                    comp.OldFixtureLayers.Add(fixture.CollisionLayer);
                }
            }
            comp.HadContentEye = EntityManager.HasComponent<ContentEyeComponent>(uid);
            comp.HadMovementIgnoreGravity = EntityManager.HasComponent<MovementIgnoreGravityComponent>(uid);
            comp.HadCanMoveInAir = EntityManager.HasComponent<CanMoveInAirComponent>(uid);
        }

        private void SaveImmunities(EntityUid uid, GhostTargetingComponent comp)
        {
            comp.HadZombieImmune = EntityManager.HasComponent<ZombieImmuneComponent>(uid);
            comp.HadBreathingImmunity = EntityManager.HasComponent<BreathingImmunityComponent>(uid);
            comp.HadPressureImmunity = EntityManager.HasComponent<PressureImmunityComponent>(uid);
            comp.HadActiveListener = EntityManager.HasComponent<ActiveListenerComponent>(uid);
            comp.HadWeakToHoly = EntityManager.HasComponent<WeakToHolyComponent>(uid);
            comp.HadCrematoriumImmune = EntityManager.HasComponent<CrematoriumImmuneComponent>(uid);
        }

        private void AddGhostComponents(EntityUid uid, GhostTargetingComponent comp)
        {
            // Додаткові компоненти
            if (!comp.HadFTLSmashImmune)
    EntityManager.EnsureComponent<FTLSmashImmuneComponent>(uid);
            if (!comp.HadUniversalLanguageSpeaker)
                EntityManager.EnsureComponent<UniversalLanguageSpeakerComponent>(uid);
            if (!comp.HadExaminer)
                EntityManager.EnsureComponent<ExaminerComponent>(uid);
            if (!comp.HadSpeechDead)
            {
                var speech = EntityManager.EnsureComponent<SpeechComponent>(uid);
                speech.SpeechVerb = "Dead";
            }

            EntityManager.EnsureComponent<MovementIgnoreGravityComponent>(uid);
            EntityManager.EnsureComponent<CanMoveInAirComponent>(uid);
            EntityManager.EnsureComponent<ContentEyeComponent>(uid);
            EntityManager.EnsureComponent<PhysicsComponent>(uid);
        }

        private void AddImmunities(EntityUid uid, GhostTargetingComponent comp)
        {
            if (!comp.HadZombieImmune)
                EntityManager.EnsureComponent<ZombieImmuneComponent>(uid);
            if (!comp.HadBreathingImmunity)
                EntityManager.EnsureComponent<BreathingImmunityComponent>(uid);
            if (!comp.HadPressureImmunity)
                EntityManager.EnsureComponent<PressureImmunityComponent>(uid);
            if (!comp.HadActiveListener)
                EntityManager.EnsureComponent<ActiveListenerComponent>(uid);
            if (!comp.HadWeakToHoly)
                EntityManager.EnsureComponent<WeakToHolyComponent>(uid).AlwaysTakeHoly = true;
            if (!comp.HadCrematoriumImmune)
                EntityManager.EnsureComponent<CrematoriumImmuneComponent>(uid);
        }

        private void SetupEyePhysicsVisibility(EntityUid uid, GhostTargetingComponent comp, bool ghostMode)
        {
            var visSys = EntityManager.System<VisibilitySystem>();
            if (ghostMode)
            {
                // Eye
                if (EntityManager.TryGetComponent<EyeComponent>(uid, out var eyeComp) && eyeComp != null)
                {
                    var eyeSys = EntityManager.System<EyeSystem>();
                    var mask = eyeComp.VisibilityMask | (int)Content.Shared.Eye.VisibilityFlags.TargetingGhost;
                    mask |= (int)Content.Shared.Eye.VisibilityFlags.Ghost;
                    eyeSys.SetDrawFov(uid, false, eyeComp);
                    eyeSys.SetVisibilityMask(uid, mask, eyeComp);
                }
                // Physics
                if (EntityManager.TryGetComponent(uid, out PhysicsComponent? physics2) && physics2 != null)
                {
                    var physicsSys = EntityManager.System<SharedPhysicsSystem>();
                    physicsSys.SetCanCollide(uid, false, body: physics2);
                    physicsSys.SetBodyType(uid, Robust.Shared.Physics.BodyType.KinematicController, body: physics2);
                }
                // Visibility
                var visComp2 = EntityManager.EnsureComponent<VisibilityComponent>(uid);
                visSys.RemoveLayer((uid, visComp2), (int)Content.Shared.Eye.VisibilityFlags.Normal, false);
                visSys.AddLayer((uid, visComp2), (int)Content.Shared.Eye.VisibilityFlags.TargetingGhost, false);
                visSys.RefreshVisibility(uid, visibilityComponent: visComp2);
            }
            else
            {
                // Eye
                if (EntityManager.TryGetComponent<EyeComponent>(uid, out var eyeComp) && eyeComp != null)
                {
                    var eyeSys = EntityManager.System<EyeSystem>();
                    var mask = eyeComp.VisibilityMask | (int)Content.Shared.Eye.VisibilityFlags.TargetingGhost;
                    mask &= ~(int)Content.Shared.Eye.VisibilityFlags.Ghost;
                    if (comp.OldDrawFov.HasValue)
                        eyeSys.SetDrawFov(uid, comp.OldDrawFov.Value, eyeComp);
                    eyeSys.SetVisibilityMask(uid, mask, eyeComp);
                }
                // Physics
                if (EntityManager.TryGetComponent(uid, out PhysicsComponent? physics) && physics != null)
                {
                    var physicsSys = EntityManager.System<SharedPhysicsSystem>();
                    if (comp.OldCanCollide.HasValue)
                        physicsSys.SetCanCollide(uid, comp.OldCanCollide.Value, body: physics);
                    if (comp.OldBodyType.HasValue)
                        physicsSys.SetBodyType(uid, comp.OldBodyType.Value, body: physics);
                }
                // Visibility
                if (comp.OldVisibilityLayers.HasValue)
                {
                    var visComp2 = EntityManager.EnsureComponent<VisibilityComponent>(uid);
                    visSys.RemoveLayer((uid, visComp2), (int)Content.Shared.Eye.VisibilityFlags.TargetingGhost, false);
                    visSys.RemoveLayer((uid, visComp2), (int)Content.Shared.Eye.VisibilityFlags.Normal, false);
                    ushort layers = (ushort)comp.OldVisibilityLayers.Value;
                    for (int i = 0; i < 16; i++)
                    {
                        ushort flag = (ushort)(1 << i);
                        if ((layers & flag) != 0)
                            visSys.AddLayer((uid, visComp2), flag, false);
                    }
                    visSys.RefreshVisibility(uid, visibilityComponent: visComp2);
                }
            }
        }

        private void UpdateActionIcon(EntityUid action, ToggleGhostFormActionEvent args, bool ghostMode)
        {
            if (EntityManager.TryGetComponent<InstantActionComponent>(action, out var actionComp))
            {
                if (ghostMode)
                {
                    actionComp.Icon = new Robust.Shared.Utility.SpriteSpecifier.Texture(new ResPath("Interface/Actions/eyeclose.png"));
                }
                else
                {
                    actionComp.Icon = new Robust.Shared.Utility.SpriteSpecifier.Texture(new ResPath("Interface/Actions/eyeopen.png"));
                }
            }
        }

        private void RemoveGhostComponents(EntityUid uid, GhostTargetingComponent comp)
        {
            // Додаткові компоненти
            if (!comp.HadFTLSmashImmune && EntityManager.HasComponent<FTLSmashImmuneComponent>(uid))
                EntityManager.RemoveComponent<FTLSmashImmuneComponent>(uid);
            if (!comp.HadUniversalLanguageSpeaker && EntityManager.HasComponent<UniversalLanguageSpeakerComponent>(uid))
                EntityManager.RemoveComponent<UniversalLanguageSpeakerComponent>(uid);
            if (!comp.HadExaminer && EntityManager.HasComponent<ExaminerComponent>(uid))
                EntityManager.RemoveComponent<ExaminerComponent>(uid);
            if (!comp.HadSpeechDead && EntityManager.HasComponent<SpeechComponent>(uid))
            {
                var speech = EntityManager.GetComponent<SpeechComponent>(uid);
                speech.SpeechVerb = comp.OldSpeechVerb ?? "Normal";
            }
            // Fixtures: змінити layer на GhostImpassable
            if (EntityManager.TryGetComponent(uid, out FixturesComponent? fixtures) && fixtures != null)
            {
                var fixtureSys = EntityManager.System<FixtureSystem>();
                int i = 0;
                var toUpdate = new List<(string id, Robust.Shared.Physics.Dynamics.Fixture oldFixture, int newLayer)>();
                foreach (var kvp in fixtures.Fixtures)
                {
                    var id = kvp.Key;
                    var fixture = kvp.Value;
                    int newLayer = comp.IsGhost
                        ? (int)Content.Shared.Physics.CollisionGroup.GhostImpassable
                        : (comp.OldFixtureLayers != null && i < comp.OldFixtureLayers.Count ? comp.OldFixtureLayers[i] : fixture.CollisionLayer);
                    toUpdate.Add((id, fixture, newLayer));
                    i++;
                }
                foreach (var (id, oldFixture, newLayer) in toUpdate)
                {
                    // Destroy old fixture
                    fixtureSys.DestroyFixture(uid, id, oldFixture, false);
                    // Recreate with new layer
                    fixtureSys.TryCreateFixture(
                        uid,
                        oldFixture.Shape,
                        id,
                        oldFixture.Density,
                        oldFixture.Hard,
                        newLayer,
                        oldFixture.CollisionMask,
                        oldFixture.Friction,
                        oldFixture.Restitution,
                        true
                    );
                }
            }

            if (!comp.HadContentEye && EntityManager.HasComponent<ContentEyeComponent>(uid))
                EntityManager.RemoveComponent<ContentEyeComponent>(uid);
            if (!comp.HadMovementIgnoreGravity && EntityManager.HasComponent<MovementIgnoreGravityComponent>(uid))
                EntityManager.RemoveComponent<MovementIgnoreGravityComponent>(uid);
            if (!comp.HadCanMoveInAir && EntityManager.HasComponent<CanMoveInAirComponent>(uid))
                EntityManager.RemoveComponent<CanMoveInAirComponent>(uid);
            if (!comp.HadPhysics && EntityManager.HasComponent<PhysicsComponent>(uid))
                EntityManager.RemoveComponent<PhysicsComponent>(uid);
        }

        private void RemoveImmunities(EntityUid uid, GhostTargetingComponent comp)
        {
            if (!comp.HadZombieImmune && EntityManager.HasComponent<ZombieImmuneComponent>(uid))
                EntityManager.RemoveComponent<ZombieImmuneComponent>(uid);
            if (!comp.HadBreathingImmunity && EntityManager.HasComponent<BreathingImmunityComponent>(uid))
                EntityManager.RemoveComponent<BreathingImmunityComponent>(uid);
            if (!comp.HadPressureImmunity && EntityManager.HasComponent<PressureImmunityComponent>(uid))
                EntityManager.RemoveComponent<PressureImmunityComponent>(uid);
            if (!comp.HadActiveListener && EntityManager.HasComponent<ActiveListenerComponent>(uid))
                EntityManager.RemoveComponent<ActiveListenerComponent>(uid);
            if (!comp.HadWeakToHoly && EntityManager.HasComponent<WeakToHolyComponent>(uid))
                EntityManager.RemoveComponent<WeakToHolyComponent>(uid);
            if (!comp.HadCrematoriumImmune && EntityManager.HasComponent<CrematoriumImmuneComponent>(uid))
                EntityManager.RemoveComponent<CrematoriumImmuneComponent>(uid);
        }

        private void RestoreState(EntityUid uid, GhostTargetingComponent comp)
        {
            // Eye/Physics/Visibility відновлюються у SetupEyePhysicsVisibility
        }

        private void RestoreImmunities(EntityUid uid, GhostTargetingComponent comp)
        {
            // Імунітети видаляються у RemoveImmunities
        }

        private void ClearSavedState(GhostTargetingComponent comp)
        {
            comp.OldVisibilityMask = null;
            comp.OldDrawFov = null;
            comp.OldCanCollide = null;
            comp.OldBodyType = null;
            comp.HadPhysics = false;
            comp.HadContentEye = false;
            comp.HadMovementIgnoreGravity = false;
            comp.HadCanMoveInAir = false;
            comp.HadZombieImmune = false;
            comp.HadBreathingImmunity = false;
            comp.HadPressureImmunity = false;
            comp.HadActiveListener = false;
            comp.HadWeakToHoly = false;
            comp.HadCrematoriumImmune = false;
            comp.OldVisibilityLayers = null;
        }
    }
}
