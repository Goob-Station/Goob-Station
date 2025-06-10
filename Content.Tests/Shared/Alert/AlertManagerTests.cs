// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.IO;
using Content.Client.Alerts;
using Content.Shared.Alert;
using NUnit.Framework;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;
using Robust.Shared.Reflection;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Utility;
using Robust.UnitTesting;

namespace Content.Tests.Shared.Alert
{
    [TestFixture, TestOf(typeof(AlertsSystem))]
    public sealed class AlertManagerTests : RobustUnitTest
    {
        const string PROTOTYPES = @"
- type: alert
  id: LowPressure
  icons:
  - /Textures/Interface/Alerts/Pressure/lowpressure.png

- type: alert
  id: HighPressure
  icons:
  - /Textures/Interface/Alerts/Pressure/highpressure.png
";

        [Test]
        [Ignore("There is no way to load extra Systems in a unit test, fixing RobustUnitTest is out of scope.")]
        public void TestAlertManager()
        {
            var entManager = IoCManager.Resolve<IEntityManager>();
            var sysManager = entManager.EntitySysManager;
            sysManager.LoadExtraSystemType<ClientAlertsSystem>();
            var alertsSystem = sysManager.GetEntitySystem<ClientAlertsSystem>();
            IoCManager.Resolve<ISerializationManager>().Initialize();

            var reflection = IoCManager.Resolve<IReflectionManager>();
            reflection.LoadAssemblies();

            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            prototypeManager.Initialize();
            prototypeManager.LoadFromStream(new StringReader(PROTOTYPES));

            Assert.That(alertsSystem.TryGet("LowPressure", out var lowPressure));
            Assert.That(lowPressure!.Icons[0], Is.EqualTo(new SpriteSpecifier.Texture(new ("/Textures/Interface/Alerts/Pressure/lowpressure.png"))));
            Assert.That(alertsSystem.TryGet("HighPressure", out var highPressure));
            Assert.That(highPressure!.Icons[0], Is.EqualTo(new SpriteSpecifier.Texture(new ("/Textures/Interface/Alerts/Pressure/highpressure.png"))));

            Assert.That(alertsSystem.TryGet("LowPressure", out lowPressure));
            Assert.That(lowPressure!.Icons[0], Is.EqualTo(new SpriteSpecifier.Texture(new ("/Textures/Interface/Alerts/Pressure/lowpressure.png"))));
            Assert.That(alertsSystem.TryGet("HighPressure", out highPressure));
            Assert.That(highPressure!.Icons[0], Is.EqualTo(new SpriteSpecifier.Texture(new ("/Textures/Interface/Alerts/Pressure/highpressure.png"))));
        }
    }
}