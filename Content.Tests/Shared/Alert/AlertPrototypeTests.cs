// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.IO;
using Content.Shared.Alert;
using NUnit.Framework;
using Robust.Shared.IoC;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.Tests.Shared.Alert
{
    [TestFixture, TestOf(typeof(AlertPrototype))]
    public sealed class AlertPrototypeTests : ContentUnitTest
    {
        private const string Prototypes = @"
- type: alert
  id: HumanHealth
  category: Health
  icons:
  - /Textures/Interface/Alerts/Human/human.rsi/human0.png
  - /Textures/Interface/Alerts/Human/human.rsi/human1.png
  - /Textures/Interface/Alerts/Human/human.rsi/human2.png
  - /Textures/Interface/Alerts/Human/human.rsi/human3.png
  - /Textures/Interface/Alerts/Human/human.rsi/human4.png
  - /Textures/Interface/Alerts/Human/human.rsi/human5.png
  - /Textures/Interface/Alerts/Human/human.rsi/human6.png
  name: Health
  description: ""[color=green]Green[/color] good. [color=red]Red[/color] bad.""
  minSeverity: 0
  maxSeverity: 6";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            IoCManager.Resolve<ISerializationManager>().Initialize();
        }

        [Test]
        public void TestAlertKey()
        {
            Assert.That(new AlertKey("HumanHealth", null), Is.Not.EqualTo(AlertKey.ForCategory("Health")));
            Assert.That((new AlertKey(null, "Health")), Is.EqualTo(AlertKey.ForCategory("Health")));
            Assert.That((new AlertKey("Buckled", "Health")), Is.EqualTo(AlertKey.ForCategory("Health")));
        }

        [TestCase(0, "/Textures/Interface/Alerts/Human/human.rsi/human0.png")]
        [TestCase(1, "/Textures/Interface/Alerts/Human/human.rsi/human1.png")]
        [TestCase(6, "/Textures/Interface/Alerts/Human/human.rsi/human6.png")]
        public void GetsIconPath(short? severity, string expected)
        {
            var alert = GetTestPrototype();
            Assert.That(alert.GetIcon(severity), Is.EqualTo(new SpriteSpecifier.Texture(new (expected))));
        }

        [TestCase(null, "/Textures/Interface/Alerts/Human/human.rsi/human0.png")]
        [TestCase(7, "/Textures/Interface/Alerts/Human/human.rsi/human1.png")]
        public void GetsIconPathThrows(short? severity, string expected)
        {
            var alert = GetTestPrototype();

            try
            {
                alert.GetIcon(severity);
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail($"Unexpected exception: {e}");
            }
        }

        private AlertPrototype GetTestPrototype()
        {
            using TextReader stream = new StringReader(Prototypes);

            var yamlStream = new YamlStream();
            yamlStream.Load(stream);

            var document = yamlStream.Documents[0];
            var rootNode = (YamlSequenceNode) document.RootNode;
            var proto = (YamlMappingNode) rootNode[0];
            var serMan = IoCManager.Resolve<ISerializationManager>();

            return serMan.Read<AlertPrototype>(new MappingDataNode(proto));
        }
    }
}