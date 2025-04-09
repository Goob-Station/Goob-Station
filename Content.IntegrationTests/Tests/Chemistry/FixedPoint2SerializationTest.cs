// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.UnitTesting.Shared.Serialization;

namespace Content.IntegrationTests.Tests.Chemistry
{
    public sealed class FixedPoint2SerializationTest : SerializationTest
    {
        protected override Assembly[] Assemblies =>
        [
            typeof(FixedPoint2SerializationTest).Assembly
        ];

        [Test]
        public void DeserializeNullTest()
        {
            var node = ValueDataNode.Null();
            var unit = Serialization.Read<FixedPoint2?>(node);

            Assert.That(unit, Is.Null);
        }

        [Test]
        public void SerializeNullTest()
        {
            var node = Serialization.WriteValue<FixedPoint2?>(null);
            Assert.That(node.IsNull);
        }

        [Test]
        public void SerializeNullableValueTest()
        {
            var node = Serialization.WriteValue<FixedPoint2?>(FixedPoint2.New(2.5f));
#pragma warning disable NUnit2045 // Interdependent assertions
            Assert.That(node is ValueDataNode);
            Assert.That(((ValueDataNode) node).Value, Is.EqualTo("2.5"));
#pragma warning restore NUnit2045
        }

        [Test]
        public void DeserializeNullDefinitionTest()
        {
            var node = new MappingDataNode().Add("unit", ValueDataNode.Null());
            var definition = Serialization.Read<FixedPoint2TestDefinition>(node);

            Assert.That(definition.Unit, Is.Null);
        }
    }

    [DataDefinition]
    public sealed partial class FixedPoint2TestDefinition
    {
        [DataField] public FixedPoint2? Unit { get; set; } = FixedPoint2.New(5);
    }
}