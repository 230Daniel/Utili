using System;

namespace DatabaseStressTest
{
    /// <summary>
    ///     Represents a Discord snowflake, i.e. a <see cref="ulong"/> offset by the constant <see cref="Epoch"/>.
    ///     <see cref="Snowflake"/> can be implicitly casted to and from <see cref="ulong"/>.
    /// </summary>
    public readonly struct Snowflake : IConvertible, IEquatable<ulong>, IEquatable<Snowflake>, IComparable<ulong>, IComparable<Snowflake>
    {
        /// <summary>
        ///     Gets the constant epoch.
        /// </summary>
        public const ulong Epoch = 1420070400000;

        /// <summary>
        ///     Gets the wrapped <see cref="ulong"/> value.
        /// </summary>
        public ulong RawValue { get; }

        /// <summary>
        ///     Gets when this snowflake was created at.
        /// </summary>
        public DateTimeOffset CreatedAt => ToDateTimeOffset(RawValue);

        /// <summary>
        ///     Gets the internal worker ID of this snowflake.
        /// </summary>
        public byte InternalWorkerId => (byte) ((RawValue & 0x3E0000) >> 17);

        /// <summary>
        ///     Gets the internal process ID of this snowflake.
        /// </summary>
        public byte InternalProcessId => (byte) ((RawValue & 0x1F000) >> 12);

        /// <summary>
        ///     Gets the increment of this snowflake.
        /// </summary>
        public ushort Increment => (ushort) (RawValue & 0xFFF);

        /// <summary>
        ///     Instantiates a new <see cref="Snowflake"/> with the specified <see cref="ulong"/> value.
        /// </summary>
        /// <param name="rawValue"> The <see cref="ulong"/> to wrap. </param>
        public Snowflake(ulong rawValue)
        {
            RawValue = rawValue;
        }

        public bool Equals(ulong other)
            => RawValue == other;

        public bool Equals(Snowflake other)
            => RawValue == other.RawValue;

        public int CompareTo(ulong other)
            => RawValue.CompareTo(other);

        public int CompareTo(Snowflake other)
            => RawValue.CompareTo(other.RawValue);

        public override bool Equals(object obj)
        {
            if (obj is Snowflake otherSnowflake)
                return RawValue == otherSnowflake.RawValue;

            if (obj is ulong otherRawValue)
                return RawValue == otherRawValue;

            return false;
        }

        public override int GetHashCode()
            => RawValue.GetHashCode();

        public override string ToString()
            => RawValue.ToString();

        public static bool TryParse(string value, out Snowflake result)
            => TryParse(value.AsSpan(), out result);

        public static bool TryParse(ReadOnlySpan<char> value, out Snowflake result)
        {
            if (value.Length >= 15 && value.Length < 21 && ulong.TryParse(value, out var ulongResult))
            {
                result = ulongResult;
                return true;
            }

            result = default;
            return false;
        }

        public static Snowflake Parse(string value)
            => Parse(value.AsSpan());

        public static Snowflake Parse(ReadOnlySpan<char> value)
            => value.Length >= 15 && value.Length < 21
                ? ulong.Parse(value)
                : throw new FormatException();

        public static Snowflake FromDateTimeOffset(DateTimeOffset dateTimeOffset)
            => ((ulong) dateTimeOffset.ToUniversalTime().ToUnixTimeMilliseconds() - Epoch) << 22;

        public static DateTimeOffset ToDateTimeOffset(ulong id)
            => DateTimeOffset.FromUnixTimeMilliseconds((long) ((id >> 22) + Epoch));
        
        public static bool operator ==(Snowflake left, Snowflake right)
            => left.RawValue == right.RawValue;

        public static bool operator !=(Snowflake left, Snowflake right)
            => left.RawValue != right.RawValue;

        public static bool operator <(Snowflake left, Snowflake right)
            => left.CompareTo(right) < 0;

        public static bool operator <=(Snowflake left, Snowflake right)
            => left.CompareTo(right) <= 0;

        public static bool operator >(Snowflake left, Snowflake right)
            => left.CompareTo(right) > 0;

        public static bool operator >=(Snowflake left, Snowflake right)
            => left.CompareTo(right) >= 0;

        public static implicit operator Snowflake(ulong value)
            => new(value);

        public static implicit operator ulong(Snowflake value)
            => value.RawValue;

        TypeCode IConvertible.GetTypeCode()
            => RawValue.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider)
            => ((IConvertible) RawValue).ToBoolean(provider);

        byte IConvertible.ToByte(IFormatProvider provider)
            => ((IConvertible) RawValue).ToByte(provider);

        char IConvertible.ToChar(IFormatProvider provider)
            => ((IConvertible) RawValue).ToChar(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
            => ((IConvertible) RawValue).ToDateTime(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider)
            => ((IConvertible) RawValue).ToDecimal(provider);

        double IConvertible.ToDouble(IFormatProvider provider)
            => ((IConvertible) RawValue).ToDouble(provider);

        short IConvertible.ToInt16(IFormatProvider provider)
            => ((IConvertible) RawValue).ToInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider)
            => ((IConvertible) RawValue).ToInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider)
            => ((IConvertible) RawValue).ToInt64(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider)
            => ((IConvertible) RawValue).ToSByte(provider);

        float IConvertible.ToSingle(IFormatProvider provider)
            => ((IConvertible) RawValue).ToSingle(provider);

        string IConvertible.ToString(IFormatProvider provider)
            => RawValue.ToString(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
            => ((IConvertible) RawValue).ToType(conversionType, provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider)
            => ((IConvertible) RawValue).ToUInt16(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider)
            => ((IConvertible) RawValue).ToUInt32(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider)
            => ((IConvertible) RawValue).ToUInt64(provider);
    }
}
