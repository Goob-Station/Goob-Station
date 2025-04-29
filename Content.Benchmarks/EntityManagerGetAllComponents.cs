// SPDX-FileCopyrightText: 2019 ZelteHonor <gabrieldionbouchard@gmail.com>
// SPDX-FileCopyrightText: 2020 Tyler Young <tyler.young@impromptu.ninja>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using BenchmarkDotNet.Attributes;
using Moq;
using Robust.Shared.Analyzers;
using Robust.Shared.Exceptions;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Reflection;

namespace Content.Benchmarks
{
    [Virtual]
    public partial class EntityManagerGetAllComponents
    {
        private IEntityManager _entityManager;

        [Params(5000)] public int N { get; set; }

        public static void TestRun()
        {
            var x = new EntityManagerGetAllComponents
            {
                N = 500
            };
            x.Setup();
            x.Run();
        }

        [GlobalSetup]
        public void Setup()
        {
            // Initialize component manager.
            IoCManager.InitThread();

            IoCManager.Register<IEntityManager, EntityManager>();
            IoCManager.Register<IRuntimeLog, RuntimeLog>();
            IoCManager.Register<ILogManager, LogManager>();
            IoCManager.Register<IDynamicTypeFactory, DynamicTypeFactory>();
            IoCManager.Register<IEntitySystemManager, EntitySystemManager>();
            IoCManager.RegisterInstance<IReflectionManager>(new Mock<IReflectionManager>().Object);

            var dummyReg = new ComponentRegistration(
                "Dummy",
                typeof(DummyComponent),
                CompIdx.Index<DummyComponent>());

            var componentFactory = new Mock<IComponentFactory>();
            componentFactory.Setup(p => p.GetComponent<DummyComponent>()).Returns(new DummyComponent());
            componentFactory.Setup(m => m.GetIndex(typeof(DummyComponent))).Returns(CompIdx.Index<DummyComponent>());
            componentFactory.Setup(p => p.GetRegistration(It.IsAny<DummyComponent>())).Returns(dummyReg);
            componentFactory.Setup(p => p.GetAllRegistrations()).Returns(new[] { dummyReg });
            componentFactory.Setup(p => p.GetAllRefTypes()).Returns(new[] { CompIdx.Index<DummyComponent>() });

            IoCManager.RegisterInstance<IComponentFactory>(componentFactory.Object);

            IoCManager.BuildGraph();
            _entityManager = IoCManager.Resolve<IEntityManager>();
            _entityManager.Initialize();

            // Initialize N entities with one component.
            for (var i = 0; i < N; i++)
            {
                var entity = _entityManager.SpawnEntity(null, EntityCoordinates.Invalid);
                _entityManager.AddComponent<DummyComponent>(entity);
            }
        }

        [Benchmark]
        public int Run()
        {
            var count = 0;

            foreach (var _ in _entityManager.EntityQuery<DummyComponent>(true))
            {
                count += 1;
            }

            return count;
        }

        [Benchmark]
        public int Noop()
        {
            var count = 0;

            _entityManager.TryGetComponent(default, out DummyComponent _);

            return count;
        }

        private sealed partial class DummyComponent : Component
        {
        }
    }
}