/*
   Copyright 2014 David Bordoley

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
using System.Linq;
using System.Text;
using Xunit;

namespace SQLitePCL.pretty.Tests
{
    public class SQLiteValueTests
    {
        static SQLiteValueTests()
        {
            Batteries_V2.Init();
        }

        private void compare(ISQLiteValue expected, ISQLiteValue test)
        {
            Assert.Equal(test.Length, expected.Length);
            Assert.Equal(test.SQLiteType, expected.SQLiteType);

            Assert.Equal(test.ToDouble(), expected.ToDouble(), 15);
            Assert.Equal(test.ToInt64(), expected.ToInt64());
            Assert.Equal(test.ToInt(), expected.ToInt());
            Assert.Equal(test.ToString(), expected.ToString());
            Assert.Equal(test.ToBlob().ToArray(), expected.ToBlob().ToArray());
        }

        [Fact]
        public void TestToSQLiteValueExtensions()
        {
            short testShort = 2;
            Assert.Equal(testShort.ToSQLiteValue().ToShort(), testShort);

            byte testByte = 2;
            Assert.Equal(testByte.ToSQLiteValue().ToByte(), testByte);

            float testFloat = 2.0f;
            Assert.Equal(testFloat.ToSQLiteValue().ToFloat(), testFloat);

            TimeSpan testTimeSpan = new TimeSpan(100);
            Assert.Equal(testTimeSpan.ToSQLiteValue().ToTimeSpan(), testTimeSpan);

            DateTime testDateTime = DateTime.Now;
            Assert.Equal(testDateTime.ToSQLiteValue().ToDateTime(), testDateTime);

            DateTimeOffset testDateTimeOffset = new DateTimeOffset(100, TimeSpan.Zero);
            Assert.Equal(testDateTimeOffset.ToSQLiteValue().ToDateTimeOffset(), testDateTimeOffset);

            decimal testDecimal = 2.2m;
            Assert.Equal(testDecimal.ToSQLiteValue().ToDecimal(), testDecimal);

            Guid testGuid = Guid.NewGuid();
            Assert.Equal(testGuid.ToSQLiteValue().ToGuid(), testGuid);

            ushort testUShort = 1;
            Assert.Equal(testUShort.ToSQLiteValue().ToUInt16(), testUShort);

            sbyte testSByte = 1;
            Assert.Equal(testSByte.ToSQLiteValue().ToSByte(), testSByte);

            Uri uri = new Uri("http://www.example.com/path/to/resource?querystring#fragment");
            Assert.Equal(uri.ToSQLiteValue().ToUri(), uri);
        }

        [Fact]
        public void TestToSQLiteValue()
        {
            Assert.Equal(0, false.ToSQLiteValue().ToInt());
            Assert.NotEqual(0, true.ToSQLiteValue().ToInt());

            byte b = 8;
            Assert.Equal(b, b.ToSQLiteValue().ToInt());

            char c = 'c';
            Assert.Equal((long) c, c.ToSQLiteValue().ToInt());

            sbyte sb = 8;
            Assert.Equal(sb, sb.ToSQLiteValue().ToInt());

            uint u = 8;
            Assert.Equal(u, u.ToSQLiteValue().ToUInt32());
        }

        [Fact]
        public void TestNullValue()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                using (var stmt = db.PrepareStatement("SELECT null;"))
                {
                    stmt.MoveNext();
                    var expected = stmt.Current.First();
                    compare(expected, SQLiteValue.Null);
                }
            }
        }

        [Fact]
        public void TestFloatValue()
        {
            double[] tests =
            {
                1,
                1.0,
                1.11,
                1.7E+3,
                -195489100.8377,
                1.12345678901234567E100,
                -1.12345678901234567E100
            };

            using (var db = SQLite3.OpenInMemory())
            {
                foreach (var test in tests)
                {
                    db.Execute("CREATE TABLE foo (x real);");
                    db.Execute("INSERT INTO foo (x) VALUES (?)", test);

                    var rows = db.Query("SELECT x FROM foo;");
                    foreach (var row in rows)
                    {
                        var expected = row.Single();
                        var result = test.ToSQLiteValue();

                        Assert.Throws<NotSupportedException>(() => { var x = result.Length; });
                        Assert.Throws<NotSupportedException>(() => { result.ToString(); });
                        Assert.Throws<NotSupportedException>(() => { result.ToBlob(); });

                        Assert.Equal(result.SQLiteType, expected.SQLiteType);
                        Assert.Equal(result.ToInt64(), expected.ToInt64());
                        Assert.Equal(result.ToInt(), expected.ToInt());
                    }

                    db.Execute("DROP TABLE foo;");
                }
            }
        }

        [Fact]
        public void TestIntValue()
        {
            long[] tests =
                {
                    2147483647, // Max int
                    -2147483648, // Min int
                    9223372036854775807, // Max Long
                    -9223372036854775808, // Min Long
                    -1234
                };

            using (var db = SQLite3.OpenInMemory())
            {
                foreach (var test in tests)
                {
                    db.Execute("CREATE TABLE foo (x int);");
                    db.Execute("INSERT INTO foo (x) VALUES (?)", test);

                    var rows = db.Query("SELECT x FROM foo;");
                    foreach (var row in rows)
                    {
                        compare(row.Single(), test.ToSQLiteValue());
                    }

                    db.Execute("DROP TABLE foo;");
                }
            }
        }

        [Fact]
        public void TestBlobValue()
        {
            string[] tests =
                {
                    "",
                    "  1234.56",
                    " 1234.abasd",
                    "abacdd\u10FFFF",
                    "2147483647", // Max int
                    "-2147483648", // Min int
                    "9223372036854775807", // Max Long
                    "-9223372036854775808", // Min Long
                    "9923372036854775809", // Greater than max long
                    "-9923372036854775809", // Less than min long
                    "3147483648", // Long
                    "-1234",
                    // "1111111111111111111111" SQLite's result in this case is undefined
                };

            using (var db = SQLite3.OpenInMemory())
            {
                foreach (var test in tests.Select(test => Encoding.UTF8.GetBytes(test)))
                {
                    db.Execute("CREATE TABLE foo (x blob);");
                    db.Execute("INSERT INTO foo (x) VALUES (?)", test);

                    var rows = db.Query("SELECT x FROM foo;");
                    foreach (var row in rows)
                    {
                        compare(row.Single(), test.ToSQLiteValue());
                    }

                    db.Execute("DROP TABLE foo;");
                }
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        public void TestZeroBlob(int i)
        {
            using (var db = SQLite3.OpenInMemory())
            {
                var test = SQLiteValue.ZeroBlob(i);
                db.Execute("CREATE TABLE foo (x blob);");
                db.Execute("INSERT INTO foo (x) VALUES (?)", test);

                foreach (var row in db.Query("SELECT x FROM foo;"))
                {
                    compare(test, row.First());
                }
                db.Execute("DROP TABLE foo;");
            }
        }

        [Fact]
        public void TestStringValue()
        {
            string[] tests =
                {
                    "  1234.56",
                    " 1234.abasd",
                    "abacdd\u10FFFF",
                    "2147483647", // Max int
                    "-2147483648", // Min int
                    "9223372036854775807", // Max Long
                    "-9223372036854775808", // Min Long
                    "9923372036854775809", // Greater than max long
                    "-9923372036854775809", // Less than min long
                    "3147483648", // Long
                    "-1234",
                    // "1111111111111111111111" SQLite's result in this case is undefined
                };

            using (var db = SQLite3.OpenInMemory())
            {
                foreach (var test in tests)
                {
                    db.Execute("CREATE TABLE foo (x text);");
                    db.Execute("INSERT INTO foo (x) VALUES (?)", test);

                    var rows = db.Query("SELECT x FROM foo;");
                    foreach (var row in rows)
                    {
                        compare(row.Single(), test.ToSQLiteValue());
                    }

                    db.Execute("DROP TABLE foo;");
                }
            }
        }

        [Fact]
        public void TestResultSetValue()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                db.Execute("CREATE TABLE foo (w int, x text, y real, z blob, n text);");

                byte[] blob = { 1, 2 };
                db.Execute("INSERT INTO foo (w, x, y, z, n) VALUES (?,?,?,?,?)", 32, "hello", 3.14, blob, null);

                using (var stmt = db.PrepareStatement("SELECT * from foo"))
                {
                    stmt.MoveNext();
                    var row = stmt.Current;

                    Assert.Equal("main", row[0].ColumnInfo.DatabaseName);
                    Assert.Equal("foo", row[0].ColumnInfo.TableName);
                    Assert.Equal("w", row[0].ColumnInfo.OriginName);
                    Assert.Equal("w", row[0].ColumnInfo.Name);
                    Assert.Equal(SQLiteType.Integer, row[0].SQLiteType);
                    Assert.Equal(32, row[0].ToInt());

                    Assert.Equal("main", row[1].ColumnInfo.DatabaseName);
                    Assert.Equal("foo", row[1].ColumnInfo.TableName);
                    Assert.Equal("x", row[1].ColumnInfo.OriginName);
                    Assert.Equal("x", row[1].ColumnInfo.Name);
                    Assert.Equal(SQLiteType.Text, row[1].SQLiteType);
                    Assert.Equal("hello", row[1].ToString());

                    Assert.Equal("main", row[2].ColumnInfo.DatabaseName);
                    Assert.Equal("foo", row[2].ColumnInfo.TableName);
                    Assert.Equal("y", row[2].ColumnInfo.OriginName);
                    Assert.Equal("y", row[2].ColumnInfo.Name);
                    Assert.Equal(SQLiteType.Float, row[2].SQLiteType);
                    Assert.Equal(3.14, row[2].ToDouble());

                    Assert.Equal("main", row[3].ColumnInfo.DatabaseName);
                    Assert.Equal("foo", row[3].ColumnInfo.TableName);
                    Assert.Equal("z", row[3].ColumnInfo.OriginName);
                    Assert.Equal("z", row[3].ColumnInfo.Name);
                    Assert.Equal(SQLiteType.Blob, row[3].SQLiteType);
                    Assert.Equal(blob, row[3].ToBlob().ToArray());

                    Assert.Equal("main", row[4].ColumnInfo.DatabaseName);
                    Assert.Equal("foo", row[4].ColumnInfo.TableName);
                    Assert.Equal("n", row[4].ColumnInfo.OriginName);
                    Assert.Equal("n", row[4].ColumnInfo.Name);
                    Assert.Equal(SQLiteType.Null, row[4].SQLiteType);
                }

                using (var stmt = db.PrepareStatement("SELECT w AS mario FROM foo;"))
                {
                    stmt.MoveNext();
                    var row = stmt.Current;

                    Assert.Equal("w", row[0].ColumnInfo.OriginName);
                    Assert.Equal("mario", row[0].ColumnInfo.Name);
                }
            }
        }
    }
}
