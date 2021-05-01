using System;
using System.Linq;
using System.Text;
using Xunit;

namespace SQLitePCL.pretty.Tests
{
    public class ResultSetTests
    {
        static ResultSetTests()
        {
            Batteries_V2.Init();
        }

        [Fact]
        public void TestScalars()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                var query = db.Query("SELECT 1;");

                Assert.True(query.SelectScalarBool().First());
                Assert.Equal(1, query.SelectScalarByte().First());
                Assert.Equal(new DateTime(1), query.SelectScalarDateTime().First());
                Assert.Equal(new DateTimeOffset(1, TimeSpan.Zero), query.SelectScalarDateTimeOffset().First());
                Assert.Equal(1m, query.SelectScalarDecimal().First());
                Assert.Equal(1, query.SelectScalarDouble().First());
                Assert.Equal(1, query.SelectScalarFloat().First());
                Assert.Equal(1, query.SelectScalarInt().First());
                Assert.Equal(1, query.SelectScalarInt64().First());
                Assert.Equal(1, query.SelectScalarSByte().First());
                Assert.Equal(1, query.SelectScalarShort().First());
                Assert.Equal("1", query.SelectScalarString().First());
                Assert.Equal(new TimeSpan(1), query.SelectScalarTimeSpan().First());
                Assert.Equal(1, query.SelectScalarUInt16().First());
                Assert.Equal((uint)1, query.SelectScalarUInt32().First());

                var guid = Guid.NewGuid();
                query = db.Query("SELECT ?", guid);
                Assert.Equal(guid, query.SelectScalarGuid().First());

                var uri = new Uri("http://www.example.com/path/to/resource");
                query = db.Query("SELECT ?", uri);
                Assert.Equal(uri, query.SelectScalarUri().First());

                var blob = Encoding.UTF8.GetBytes("ab");
                foreach (var row in db.Query("SELECT ?", blob))
                {
                    Assert.Equal("ab", Encoding.UTF8.GetString(row[0].ToBlob()));
                }
            }
        }


        [Fact]
        public void TestCount()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                db.ExecuteAll(
                    @"CREATE TABLE foo (x int, v int);
                      INSERT INTO foo (x, v) VALUES (1, 2);
                      INSERT INTO foo (x, v) VALUES (2, 3);");

                foreach (var row in db.Query("select * from foo"))
                {
                    Assert.Equal(2, row.Count);
                }

                foreach (var row in db.Query("select x from foo"))
                {
                    Assert.Equal(1, row.Count);
                }
            }
        }

        [Fact]
        public void TestBracketOp()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                db.ExecuteAll(
                    @"CREATE TABLE foo (x int, v int);
                      INSERT INTO foo (x, v) VALUES (1, 2);
                      INSERT INTO foo (x, v) VALUES (2, 3);");

                foreach (var row in db.Query("select * from foo"))
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => { var x = row[-1]; });
                    Assert.Throws<ArgumentOutOfRangeException>(() => { var x = row[row.Count]; });

                    Assert.Equal(SQLiteType.Integer, row[0].SQLiteType);
                    Assert.Equal(SQLiteType.Integer, row[1].SQLiteType);
                }
            }
        }

        [Fact]
        public void TestColumns()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                foreach (var row in db.Query("SELECT 1 as a, 2 as b"))
                {
                    var columns = row.Columns();
                    Assert.Equal("a", columns[0].Name);
                    Assert.Equal("b", columns[1].Name);
                    Assert.Equal(2, columns.Count);

                    var count = row.Columns().Count;

                    Assert.Equal(2, count);
                }
            }
        }
    }
}

