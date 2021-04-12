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
using System.IO;
using Xunit;

namespace SQLitePCL.pretty.Tests
{
    public class BindParametersTests
    {
        static BindParametersTests()
        {
            Batteries_V2.Init();
        }

        [Fact]
        public void TestBindOnDisposedStatement()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                db.Execute("CREATE TABLE foo (v int);");

                IReadOnlyOrderedDictionary<string, IBindParameter> bindParams;

                using (var stmt = db.PrepareStatement("INSERT INTO foo (v) VALUES (?)"))
                {
                    bindParams = stmt.BindParameters;
                }

                Assert.Throws<ObjectDisposedException>(() => { var x = bindParams[0]; });
            }
        }

        [Fact]
        public void TestBindObject()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                db.Execute("CREATE TABLE foo (v int);");
                using (var stmt = db.PrepareStatement("INSERT INTO foo (v) VALUES (?)"))
                {
                    var stream = new MemoryStream();
                    stream.Dispose();
                    Assert.Throws<ArgumentException>(() => stmt.BindParameters[0].Bind(stream));
                    Assert.Throws<ArgumentException>(() => stmt.BindParameters[0].Bind(new object()));
                }

                using (var stmt = db.PrepareStatement("SELECT ?"))
                {
                    stmt.Reset();
                    stmt.ClearBindings();
                    stmt.BindParameters[0].Bind((object)DateTime.MaxValue);
                    stmt.MoveNext();
                    Assert.Equal(DateTime.MaxValue, stmt.Current[0].ToDateTime());

                    stmt.Reset();
                    stmt.ClearBindings();
                    stmt.BindParameters[0].Bind((object)DateTimeOffset.MaxValue);
                    stmt.MoveNext();
                    Assert.Equal(DateTimeOffset.MaxValue, stmt.Current[0].ToDateTimeOffset());

                    stmt.Reset();
                    stmt.ClearBindings();
                    stmt.BindParameters[0].Bind((object)TimeSpan.Zero);
                    stmt.MoveNext();
                    Assert.Equal(TimeSpan.Zero, stmt.Current[0].ToTimeSpan());
                }
            }
        }

        [Fact]
        public void TestBindExtensions()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                using (var stmt = db.PrepareStatement("SELECT ?"))
                {
                    stmt.Reset();
                    stmt.ClearBindings();
                    stmt.BindParameters[0].Bind(true);
                    stmt.MoveNext();
                    Assert.True(stmt.Current[0].ToBool());

                    stmt.Reset();
                    stmt.ClearBindings();
                    stmt.BindParameters[0].Bind(TimeSpan.Zero);
                    stmt.MoveNext();
                    Assert.Equal(TimeSpan.Zero, stmt.Current[0].ToTimeSpan());

                    stmt.Reset();
                    stmt.ClearBindings();
                    stmt.BindParameters[0].Bind(1.1m);
                    stmt.MoveNext();
                    Assert.Equal(new decimal(1.1), stmt.Current[0].ToDecimal());

                    stmt.Reset();
                    stmt.ClearBindings();
                    stmt.BindParameters[0].Bind(DateTime.MaxValue);
                    stmt.MoveNext();
                    Assert.Equal(DateTime.MaxValue, stmt.Current[0].ToDateTime());

                    stmt.Reset();
                    stmt.ClearBindings();
                    stmt.BindParameters[0].Bind(DateTimeOffset.MaxValue);
                    stmt.MoveNext();
                    Assert.Equal(DateTimeOffset.MaxValue, stmt.Current[0].ToDateTimeOffset());
                }
            }
        }

        [Fact]
        public void TestBindSQLiteValue()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                db.Execute("CREATE TABLE foo (v int);");
                using (var stmt = db.PrepareStatement("SELECT ?"))
                {
                    var param = stmt.BindParameters[0];
                    param.Bind(SQLiteValue.Null);
                    stmt.MoveNext();
                    var result = stmt.Current[0];
                    Assert.Equal(SQLiteType.Null, result.SQLiteType);

                    stmt.Reset();
                    param.Bind(Array.Empty<byte>().ToSQLiteValue());
                    stmt.MoveNext();
                    result = stmt.Current[0];
                    Assert.Equal(SQLiteType.Blob, result.SQLiteType);
                    Assert.Equal(Array.Empty<byte>(), result.ToBlob().ToArray());

                    stmt.Reset();
                    param.Bind("test".ToSQLiteValue());
                    stmt.MoveNext();
                    result = stmt.Current[0];
                    Assert.Equal(SQLiteType.Text, result.SQLiteType);
                    Assert.Equal("test", result.ToString());

                    stmt.Reset();
                    param.Bind((1).ToSQLiteValue());
                    stmt.MoveNext();
                    result = stmt.Current[0];
                    Assert.Equal(SQLiteType.Integer, result.SQLiteType);
                    Assert.Equal(1, result.ToInt64());

                    stmt.Reset();
                    param.Bind((0.0).ToSQLiteValue());
                    stmt.MoveNext();
                    result = stmt.Current[0];
                    Assert.Equal(SQLiteType.Float, result.SQLiteType);
                    Assert.Equal(0, result.ToInt());
                }
            }
        }
    }
}
