/*
   Copyright 2014 David Bordoley
   Copyright 2014 Zumero, LLC

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Buffers.Text;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SQLitePCL.pretty
{
    /// <summary>
    /// Extensions methods for creating instances of <see cref="ISQLiteValue"/>.
    /// </summary>
    public static class SQLiteValue
    {
        /// <summary>
        /// The SQLite null value.
        /// </summary>
        public static ISQLiteValue Null { get; } = new NullValue();

        /// <summary>
        /// Create a SQLite zeroblob of the specified length.
        /// </summary>
        /// <remarks>This method does not allocate any memory,
        /// therefore the zero blobs can safely be of arbitrary length.</remarks>
        /// <param name="length">The length of the zero blob</param>
        /// <returns>An ISQLiteValue representing the zero blob.</returns>
        public static ISQLiteValue ZeroBlob(int length)
        {
            Contract.Requires(length >= 0);
            return new ZeroBlob(length);
        }

        /// <summary>
        /// Converts an <see cref="int"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this int This) =>
            Convert.ToInt64(This).ToSQLiteValue();

        /// <summary>
        /// Converts an <see cref="short"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this short This) =>
            Convert.ToInt64(This).ToSQLiteValue();

        /// <summary>
        /// Converts a <see cref="bool"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this bool This) =>
            Convert.ToInt64(This).ToSQLiteValue();

        /// <summary>
        /// Converts a <see cref="byte"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this byte This) =>
            Convert.ToInt64(This).ToSQLiteValue();

        /// <summary>
        /// Converts a <see cref="char"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this char This) =>
            Convert.ToInt64(This).ToSQLiteValue();

        /// <summary>
        /// Converts a <see cref="sbyte"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this sbyte This) =>
            Convert.ToInt64(This).ToSQLiteValue();

        /// <summary>
        /// Converts a <see cref="UInt32"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this UInt32 This) =>
            Convert.ToInt64(This).ToSQLiteValue();

        /// <summary>
        /// Converts an <see cref="UInt16"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this UInt16 This) =>
            Convert.ToInt64(This).ToSQLiteValue();

        /// <summary>
        /// Converts a <see cref="long"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this long This) =>
            new IntValue(This);

        /// <summary>
        /// Converts a <see cref="double"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this double This) =>
            new FloatValue(This);

        /// <summary>
        /// Converts an <see cref="float"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this float This) =>
            Convert.ToDouble(This).ToSQLiteValue();

        /// <summary>
        /// Converts a <see cref="string"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this string This)
        {
            Contract.Requires(This != null);
            return new StringValue(This);
        }

        /// <summary>
        /// Converts a byte array to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the blob.</returns>
        public static ISQLiteValue ToSQLiteValue(this byte[] This)
        {
            Contract.Requires(This != null);
            return new BlobValue(This);
        }

        /// <summary>
        /// Converts an <see cref="TimeSpan"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this TimeSpan This) =>
            This.Ticks.ToSQLiteValue();

        /// <summary>
        /// Converts an <see cref="DateTime"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this DateTime This) =>
            This.Ticks.ToSQLiteValue();

        /// <summary>
        /// Converts an <see cref="DateTimeOffset"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this DateTimeOffset This) =>
            This.ToOffset(TimeSpan.Zero).Ticks.ToSQLiteValue();

        /// <summary>
        /// Converts an <see cref="decimal"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this decimal This) =>
            Convert.ToDouble(This).ToSQLiteValue();

        /// <summary>
        /// Converts an <see cref="Guid"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this Guid This) =>
            This.ToString().ToSQLiteValue();

        /// <summary>
        /// Converts an <see cref="Uri"/> to an <see cref="ISQLiteValue"/>.
        /// </summary>
        /// <param name="This">The value to convert</param>
        /// <returns>A ISQLiteValue representing the value.</returns>
        public static ISQLiteValue ToSQLiteValue(this Uri This)
        {
            Contract.Requires(This != null);
            return This.ToString().ToSQLiteValue();
        }

        internal static NativeValue ToSQLiteValue(this sqlite3_value This) =>
            new NativeValue(This);

        internal static ResultSetValue ResultSetValueAt(this StatementImpl This, int index) =>
            new ResultSetValue(This, index);

        internal static void SetResult(this sqlite3_context ctx, ISQLiteValue value)
        {
            switch (value.SQLiteType)
            {
                case SQLiteType.Blob:
                    if (value is ZeroBlob)
                    {
                        raw.sqlite3_result_zeroblob(ctx, value.Length);
                    }
                    else
                    {
                        raw.sqlite3_result_blob(ctx, value.ToBlob());
                    }
                    return;

                case SQLiteType.Null:
                    raw.sqlite3_result_null(ctx);
                    return;

                case SQLiteType.Text:
                    raw.sqlite3_result_text(ctx, value.ToString());
                    return;

                case SQLiteType.Float:
                    raw.sqlite3_result_double(ctx, value.ToDouble());
                    return;

                case SQLiteType.Integer:
                    raw.sqlite3_result_int64(ctx, value.ToInt64());
                    return;
            }
        }

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="bool"/>.
        /// </summary>
        public static bool ToBool(this ISQLiteValue This)
        {
            Contract.Requires(This != null);
            return Convert.ToBoolean(This.ToInt64());
        }

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="float"/>.
        /// </summary>
        public static float ToFloat(this ISQLiteValue This)
        {
            Contract.Requires(This != null);
            return (float)This.ToDouble();
        }

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="TimeSpan"/>.
        /// </summary>
        public static TimeSpan ToTimeSpan(this ISQLiteValue This)
        {
            Contract.Requires(This != null);
            return new TimeSpan(This.ToInt64());
        }

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="DateTime"/>.
        /// </summary>
        public static DateTime ToDateTime(this ISQLiteValue This)
        {
            Contract.Requires(This != null);
            return new DateTime(This.ToInt64());
        }

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="DateTimeOffset"/>.
        /// </summary>
        public static DateTimeOffset ToDateTimeOffset(this ISQLiteValue This)
        {
            Contract.Requires(This != null);
            return new DateTimeOffset(This.ToInt64(), TimeSpan.Zero);
        }

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="uint"/>.
        /// </summary>
        public static uint ToUInt32(this ISQLiteValue This)
        {
            Contract.Requires(This != null);
            return Convert.ToUInt32(This.ToInt64());
        }

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="decimal"/>.
        /// </summary>
        public static decimal ToDecimal(this ISQLiteValue This)
        {
            Contract.Requires(This != null);
            return Convert.ToDecimal(This.ToDouble());
        }

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="byte"/>.
        /// </summary>
        public static byte ToByte(this ISQLiteValue This)
        {
            Contract.Requires(This != null);
            return Convert.ToByte(This.ToInt64());
        }

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="UInt16"/>.
        /// </summary>
        public static UInt16 ToUInt16(this ISQLiteValue This)
        {
            Contract.Requires(This != null);
            return Convert.ToUInt16(This.ToInt64());
        }

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="short"/>.
        /// </summary>
        public static short ToShort(this ISQLiteValue This)
        {
            Contract.Requires(This != null);
            return Convert.ToInt16(This.ToInt64());
        }

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="sbyte"/>.
        /// </summary>
        public static sbyte ToSByte(this ISQLiteValue This)
        {
            Contract.Requires(This != null);
            return Convert.ToSByte(This.ToInt64());
        }

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="Guid"/>.
        /// </summary>
        public static Guid ToGuid(this ISQLiteValue This)
        {
            Contract.Requires(This != null);

            if (!Utf8Parser.TryParse(This.ToBlob(), out Guid value, out _))
            {
                ThrowHelper.ThrowFormatException();
            }

            return value;
        }

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="Uri"/>.
        /// </summary>
        public static Uri ToUri(this ISQLiteValue This)
        {
            Contract.Requires(This != null);

            var text = This.ToString();
            return new Uri(text);
        }

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="bool"/>.
        /// </summary>
        public static bool ToBool(this ResultSetValue This)
            => Convert.ToBoolean(This.ToInt64());

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="float"/>.
        /// </summary>
        public static float ToFloat(this ResultSetValue This)
            => (float)This.ToDouble();

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="TimeSpan"/>.
        /// </summary>
        public static TimeSpan ToTimeSpan(this ResultSetValue This)
            => new TimeSpan(This.ToInt64());

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="DateTime"/>.
        /// </summary>
        public static DateTime ToDateTime(this ResultSetValue This)
            => new DateTime(This.ToInt64());

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="DateTimeOffset"/>.
        /// </summary>
        public static DateTimeOffset ToDateTimeOffset(this ResultSetValue This)
            => new DateTimeOffset(This.ToInt64(), TimeSpan.Zero);

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="uint"/>.
        /// </summary>
        public static uint ToUInt32(this ResultSetValue This)
            => Convert.ToUInt32(This.ToInt64());

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="decimal"/>.
        /// </summary>
        public static decimal ToDecimal(this ResultSetValue This)
            => Convert.ToDecimal(This.ToDouble());

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="byte"/>.
        /// </summary>
        public static byte ToByte(this ResultSetValue This)
            => Convert.ToByte(This.ToInt64());

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="UInt16"/>.
        /// </summary>
        public static UInt16 ToUInt16(this ResultSetValue This)
            => Convert.ToUInt16(This.ToInt64());

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="short"/>.
        /// </summary>
        public static short ToShort(this ResultSetValue This)
            => Convert.ToInt16(This.ToInt64());

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="sbyte"/>.
        /// </summary>
        public static sbyte ToSByte(this ResultSetValue This)
            => Convert.ToSByte(This.ToInt64());

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="Guid"/>.
        /// </summary>
        public static Guid ToGuid(this ResultSetValue This)
        {
            if (!Utf8Parser.TryParse(This.ToBlob(), out Guid value, out _))
            {
                ThrowHelper.ThrowFormatException();
            }

            return value;
        }

        /// <summary>
        /// Returns the SQLiteValue as a <see cref="Uri"/>.
        /// </summary>
        public static Uri ToUri(this ResultSetValue This)
        {
            var text = This.ToString();
            return new Uri(text);
        }
    }

    internal readonly struct NativeValue : ISQLiteValue
    {
        private readonly sqlite3_value value;

        internal NativeValue(sqlite3_value value)
        {
            this.value = value;
        }

        public SQLiteType SQLiteType =>
            (SQLiteType)raw.sqlite3_value_type(value);

        public int Length =>
            raw.sqlite3_value_bytes(value);

        public ReadOnlySpan<byte> ToBlob() =>
            raw.sqlite3_value_blob(value);

        public double ToDouble() =>
            raw.sqlite3_value_double(value);

        public int ToInt() =>
            raw.sqlite3_value_int(value);

        public long ToInt64() =>
            raw.sqlite3_value_int64(value);

        public override string ToString() =>
            raw.sqlite3_value_text(value).utf8_to_string() ?? string.Empty;
    }

    // Type coercion rules
    // http://www.sqlite.org/capi3ref.html#sqlite3_column_blob
    internal readonly struct NullValue : ISQLiteValue
    {
        public SQLiteType SQLiteType => SQLiteType.Null;

        public int Length => 0;

        public ReadOnlySpan<byte> ToBlob() => Span<byte>.Empty;

        public double ToDouble() => 0.0;

        public int ToInt() => 0;

        public long ToInt64() => 0;

        public override string ToString() => string.Empty;
    }

    internal readonly struct IntValue : ISQLiteValue
    {
        private readonly long value;

        internal IntValue(long value)
        {
            this.value = value;
        }

        public SQLiteType SQLiteType => SQLiteType.Integer;

        public int Length =>
            this.ToBlob().Length;

        public ReadOnlySpan<byte> ToBlob()
        {
            Span<byte> span = new byte[20];
            if (!Utf8Formatter.TryFormat(value, span, out var bytesWritten))
            {
                ThrowHelper.ThrowFormatException();
            }

            return span.Slice(0, bytesWritten);
        }

        public double ToDouble() => value;

        public int ToInt() => (int)value;

        public long ToInt64() => value;

        public override string ToString() => value.ToString();
    }

    internal readonly struct FloatValue : ISQLiteValue
    {
        private readonly double value;

        internal FloatValue(double value)
        {
            this.value = value;
        }

        public SQLiteType SQLiteType => SQLiteType.Float;

        public int Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        // Casting to a BLOB consists of first casting the value to TEXT
        // in the encoding of the database connection, then interpreting
        // the resulting byte sequence as a BLOB instead of as TEXT.
        public ReadOnlySpan<byte> ToBlob()
        {
            throw new NotSupportedException();
        }

        public double ToDouble() => value;

        public int ToInt() => (int)this.ToInt64();

        public long ToInt64()
        {
            if (this.value > Int64.MaxValue)
            {
                return Int64.MaxValue;
            }
            else if (this.value < Int64.MinValue)
            {
                return Int64.MinValue;
            }
            else
            {
                return (long)value;
            }
        }

        public override string ToString() => throw new NotSupportedException();
    }

    internal readonly struct StringValue : ISQLiteValue
    {
        private static readonly Regex isNegative = new Regex("^[ ]*[-]");
        private static readonly Regex stringToDoubleCast = new Regex("^[ ]*([-]?)[0-9]+([.][0-9]+)?");
        private static readonly Regex stringToLongCast = new Regex("^[ ]*([-]?)[0-9]+");

        private readonly string value;

        internal StringValue(string value)
        {
            this.value = value;
        }

        public SQLiteType SQLiteType => SQLiteType.Text;

        public int Length => Encoding.UTF8.GetByteCount(value);

        public ReadOnlySpan<byte> ToBlob() => Encoding.UTF8.GetBytes(value);

        // When casting a TEXT value to REAL, the longest possible prefix
        // of the value that can be interpreted as a real number is extracted
        // from the TEXT value and the remainder ignored. Any leading spaces
        // in the TEXT value are ignored when converging from TEXT to REAL.
        // If there is no prefix that can be interpreted as a real number,
        // the result of the conversion is 0.0.
        public double ToDouble()
        {
            var result = stringToDoubleCast.Match(this.value);
            if (result.Success)
            {
                if (double.TryParse(result.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out double parsed))
                {
                    return parsed;
                }
            }

            return 0.0;
        }

        public int ToInt() => (int)this.ToInt64();

        // When casting a TEXT value to INTEGER, the longest possible prefix of
        // the value that can be interpreted as an integer number is extracted from
        // the TEXT value and the remainder ignored. Any leading spaces in the TEXT
        // value when converting from TEXT to INTEGER are ignored. If there is no
        // prefix that can be interpreted as an integer number, the result of the
        // conversion is 0. The CAST operator understands decimal integers only —
        // conversion of hexadecimal integers stops at the "x" in the "0x" prefix
        // of the hexadecimal integer string and thus result of the CAST is always zero.
        public long ToInt64()
        {
            var result = stringToLongCast.Match(this.value);
            if (result.Success)
            {
                if (long.TryParse(result.Value, out long parsed))
                {
                    return parsed;
                }
                else if (isNegative.Match(this.value).Success)
                {
                    return long.MinValue;
                }
                else
                {
                    return long.MaxValue;
                }
            }

            return 0;
        }

        public override string ToString() => value;
    }

    internal readonly struct BlobValue : ISQLiteValue
    {
        private readonly byte[] value;

        internal BlobValue(byte[] value)
        {
            this.value = value;
        }

        public SQLiteType SQLiteType => SQLiteType.Blob;

        public int Length => value.Length;

        public ReadOnlySpan<byte> ToBlob() => value;

        // When casting a BLOB value to a REAL, the value is first converted to TEXT.
        public double ToDouble() =>
            this.ToString().ToSQLiteValue().ToDouble();

        public int ToInt() => (int)this.ToInt64();

        // When casting a BLOB value to INTEGER, the value is first converted to TEXT.
        public long ToInt64() =>
            this.ToString().ToSQLiteValue().ToInt64();

        public override string ToString() =>
            Encoding.UTF8.GetString(value);
    }

    /// <summary>
    /// An <see cref="ISQLiteValue"/> that includes <see cref="ColumnInfo"/> about the value.
    /// </summary>
    public readonly struct ResultSetValue : ISQLiteValue
    {
        private readonly StatementImpl stmt;
        private readonly int index;

        internal ResultSetValue(StatementImpl stmt, int index)
        {
            this.stmt = stmt;
            this.index = index;
        }

        /// <inheritdoc />
        public ColumnInfo ColumnInfo =>
            ColumnInfo.Create(stmt, index);

        /// <inheritdoc />
        public SQLiteType SQLiteType =>
            (SQLiteType)raw.sqlite3_column_type(stmt.sqlite3_stmt, index);

        /// <inheritdoc />
        public int Length =>
            raw.sqlite3_column_bytes(stmt.sqlite3_stmt, index);

        /// <inheritdoc />
        public ReadOnlySpan<byte> ToBlob() =>
            raw.sqlite3_column_blob(stmt.sqlite3_stmt, index);

        /// <inheritdoc />
        public double ToDouble() =>
            raw.sqlite3_column_double(stmt.sqlite3_stmt, index);

        /// <inheritdoc />
        public int ToInt() =>
            raw.sqlite3_column_int(stmt.sqlite3_stmt, index);

        /// <inheritdoc />
        public long ToInt64() =>
            raw.sqlite3_column_int64(stmt.sqlite3_stmt, index);

        /// <inheritdoc />
        public override string ToString() =>
            raw.sqlite3_column_text(stmt.sqlite3_stmt, index).utf8_to_string() ?? string.Empty;
    }

    internal readonly struct ZeroBlob : ISQLiteValue
    {
        private readonly int length;

        internal ZeroBlob(int length)
        {
            this.length = length;
        }

        public SQLiteType SQLiteType => SQLiteType.Blob;

        public int Length => length;

        public ReadOnlySpan<byte> ToBlob() => new byte[length];

        public double ToDouble() => 0;

        public int ToInt() => 0;

        public long ToInt64() => 0;

        public override string ToString()
            => Encoding.UTF8.GetString(ToBlob());
    }
}
