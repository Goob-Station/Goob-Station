// SPDX-FileCopyrightText: 2020 ComicIronic <comicironic@gmail.com>
// SPDX-FileCopyrightText: 2020 PrPleGoo <felix.leeuwen@gmail.com>
// SPDX-FileCopyrightText: 2020 Tyler Young <tyler.young@impromptu.ninja>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2021 py01 <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 py01 <pyronetics01@gmail.com>
// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Sam Weaver <weaversam8@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Globalization;
using System.Linq;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.FixedPoint
{
    /// <summary>
    ///     Represents a quantity of something, to a precision of 0.01.
    ///     To enforce this level of precision, floats are shifted by 2 decimal points, rounded, and converted to an int.
    /// </summary>
    [Serializable, CopyByRef]
    public struct FixedPoint4 : ISelfSerialize, IComparable<FixedPoint4>, IEquatable<FixedPoint4>, IFormattable
    {
        public long Value { get; private set; }
        private const long Shift = 4;
        private const long ShiftConstant = 10000; // Must be equal to pow(10, Shift)

        public static FixedPoint4 MaxValue { get; } = new(long.MaxValue);
        public static FixedPoint4 Epsilon { get; } = new(1);
        public static FixedPoint4 Zero { get; } = new(0);

        // This value isn't picked by any proper testing, don't @ me.
        private const float FloatEpsilon = 0.00001f;

#if DEBUG
        static FixedPoint4()
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            DebugTools.Assert(Math.Pow(10, Shift) == ShiftConstant, "ShiftConstant must be equal to pow(10, Shift)");
        }
#endif

        private readonly double ShiftDown()
        {
            return Value / (double) ShiftConstant;
        }

        private FixedPoint4(long value)
        {
            Value = value;
        }

        public static FixedPoint4 New(long value)
        {
            return new(value * ShiftConstant);
        }
        public static FixedPoint4 FromTenThousandths(long value) => new(value);

        public static FixedPoint4 New(float value)
        {
            return new((long) ApplyFloatEpsilon(value * ShiftConstant));
        }

        private static float ApplyFloatEpsilon(float value)
        {
            return value + FloatEpsilon * Math.Sign(value);
        }

        private static double ApplyFloatEpsilon(double value)
        {
            return value + FloatEpsilon * Math.Sign(value);
        }

        /// <summary>
        /// Create the closest <see cref="FixedPoint4"/> for a float value, always rounding up.
        /// </summary>
        public static FixedPoint4 NewCeiling(float value)
        {
            return new((long) MathF.Ceiling(value * ShiftConstant));
        }

        public static FixedPoint4 New(double value)
        {
            return new((long) ApplyFloatEpsilon(value * ShiftConstant));
        }

        public static FixedPoint4 New(string value)
        {
            return New(Parse.Float(value));
        }

        public static FixedPoint4 operator +(FixedPoint4 a) => a;

        public static FixedPoint4 operator -(FixedPoint4 a) => new(-a.Value);

        public static FixedPoint4 operator +(FixedPoint4 a, FixedPoint4 b)
            => new(a.Value + b.Value);

        public static FixedPoint4 operator -(FixedPoint4 a, FixedPoint4 b)
            => new(a.Value - b.Value);

        public static FixedPoint4 operator *(FixedPoint4 a, FixedPoint4 b)
        {
            return new(b.Value * a.Value / ShiftConstant);
        }

        public static FixedPoint4 operator *(FixedPoint4 a, float b)
        {
            return new((long) ApplyFloatEpsilon(a.Value * b));
        }

        public static FixedPoint4 operator *(FixedPoint4 a, double b)
        {
            return new((long) ApplyFloatEpsilon(a.Value * b));
        }

        public static FixedPoint4 operator *(FixedPoint4 a, long b)
        {
            return new(a.Value * b);
        }

        public static FixedPoint4 operator /(FixedPoint4 a, FixedPoint4 b)
        {
            return new((long) (ShiftConstant * (long) a.Value / b.Value));
        }

        public static FixedPoint4 operator /(FixedPoint4 a, float b)
        {
            return new((long) ApplyFloatEpsilon(a.Value / b));
        }

        public static bool operator <=(FixedPoint4 a, long b)
        {
            return a <= New(b);
        }

        public static bool operator >=(FixedPoint4 a, long b)
        {
            return a >= New(b);
        }

        public static bool operator <(FixedPoint4 a, long b)
        {
            return a < New(b);
        }

        public static bool operator >(FixedPoint4 a, long b)
        {
            return a > New(b);
        }

        public static bool operator ==(FixedPoint4 a, long b)
        {
            return a == New(b);
        }

        public static bool operator !=(FixedPoint4 a, long b)
        {
            return a != New(b);
        }

        public static bool operator ==(FixedPoint4 a, FixedPoint4 b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(FixedPoint4 a, FixedPoint4 b)
        {
            return !a.Equals(b);
        }

        public static bool operator <=(FixedPoint4 a, FixedPoint4 b)
        {
            return a.Value <= b.Value;
        }

        public static bool operator >=(FixedPoint4 a, FixedPoint4 b)
        {
            return a.Value >= b.Value;
        }

        public static bool operator <(FixedPoint4 a, FixedPoint4 b)
        {
            return a.Value < b.Value;
        }

        public static bool operator >(FixedPoint4 a, FixedPoint4 b)
        {
            return a.Value > b.Value;
        }

        public readonly float Float()
        {
            return (float) ShiftDown();
        }

        public readonly double Double()
        {
            return ShiftDown();
        }

        public readonly long Long()
        {
            return Value / ShiftConstant;
        }

        public readonly int Int()
        {
            return (int)Long();
        }

        // Implicit operators ftw
        public static implicit operator FixedPoint4(FixedPoint2 n) => New(n.Int());
        public static implicit operator FixedPoint4(float n) => New(n);
        public static implicit operator FixedPoint4(double n) => New(n);
        public static implicit operator FixedPoint4(int n) => New(n);
        public static implicit operator FixedPoint4(long n) => New(n);

        public static explicit operator FixedPoint2(FixedPoint4 n) => n.Int();
        public static explicit operator float(FixedPoint4 n) => n.Float();
        public static explicit operator double(FixedPoint4 n) => n.Double();
        public static explicit operator int(FixedPoint4 n) => n.Int();
        public static explicit operator long(FixedPoint4 n) => n.Long();

        public static FixedPoint4 Min(params FixedPoint4[] fixedPoints)
        {
            return fixedPoints.Min();
        }

        public static FixedPoint4 Min(FixedPoint4 a, FixedPoint4 b)
        {
            return a < b ? a : b;
        }

        public static FixedPoint4 Max(FixedPoint4 a, FixedPoint4 b)
        {
            return a > b ? a : b;
        }

        public static long Sign(FixedPoint4 value)
        {
            if (value < Zero)
            {
                return -1;
            }

            if (value > Zero)
            {
                return 1;
            }

            return 0;
        }

        public static FixedPoint4 Abs(FixedPoint4 a)
        {
            return FromTenThousandths(Math.Abs(a.Value));
        }

        public static FixedPoint4 Dist(FixedPoint4 a, FixedPoint4 b)
        {
            return FixedPoint4.Abs(a - b);
        }

        public static FixedPoint4 Clamp(FixedPoint4 number, FixedPoint4 min, FixedPoint4 max)
        {
            if (min > max)
            {
                throw new ArgumentException($"{nameof(min)} {min} cannot be larger than {nameof(max)} {max}");
            }

            return number < min ? min : number > max ? max : number;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is FixedPoint4 unit &&
                   Value == unit.Value;
        }

        public override readonly int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return HashCode.Combine(Value);
        }

        public void Deserialize(string value)
        {
            // TODO implement "lossless" serializer.
            // I.e., dont use floats.
            if (value == "MaxValue")
                Value = int.MaxValue;
            else
                this = New(Parse.Double(value));
        }

        public override readonly string ToString() => $"{ShiftDown().ToString(CultureInfo.InvariantCulture)}";

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return ToString();
        }

        public readonly string Serialize()
        {
            // TODO implement "lossless" serializer.
            // I.e., dont use floats.
            if (Value == int.MaxValue)
                return "MaxValue";

            return ToString();
        }

        public readonly bool Equals(FixedPoint4 other)
        {
            return Value == other.Value;
        }

        public readonly int CompareTo(FixedPoint4 other)
        {
            if (other.Value > Value)
            {
                return -1;
            }
            if (other.Value < Value)
            {
                return 1;
            }
            return 0;
        }

    }

    public static class FixedPoint4EnumerableExt
    {
        public static FixedPoint4 Sum(this IEnumerable<FixedPoint4> source)
        {
            var acc = FixedPoint4.Zero;

            foreach (var n in source)
            {
                acc += n;
            }

            return acc;
        }
    }
}