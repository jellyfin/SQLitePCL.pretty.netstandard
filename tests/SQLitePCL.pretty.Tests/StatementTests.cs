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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SQLitePCL.pretty.Tests
{
    public class StatementTests
    {
        static StatementTests()
        {
            Batteries_V2.Init();
        }

        [Fact]
        public void TestCurrent()
        {
            using (var db = SQLite3.OpenInMemory())
            using (var stmt = db.PrepareStatement("SELECT 1"))
            {
                stmt.MoveNext();
                Assert.Equal(1, stmt.Current[0].ToInt());

                var ienumCurrent = ((IEnumerator)stmt).Current;
                var ienumResultSet = (IReadOnlyList<ResultSetValue>) ienumCurrent;
                Assert.Equal(1, ienumResultSet[0].ToInt());
            }
        }

        [Fact]
        public void TestDispose()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                var stmt = db.PrepareStatement("SELECT 1");
                stmt.Dispose();

                // Test double dispose
                stmt.Dispose();

                Assert.Throws<ObjectDisposedException>(() => { var x = stmt.BindParameters; });
                Assert.Throws<ObjectDisposedException>(() => { var x = stmt.Columns; });
                Assert.Throws<ObjectDisposedException>(() => { var x = stmt.Current; });
                Assert.Throws<ObjectDisposedException>(() => { var x = stmt.SQL; });
                Assert.Throws<ObjectDisposedException>(() => { var x = stmt.IsBusy; });
                Assert.Throws<ObjectDisposedException>(() => { var x = stmt.IsReadOnly; });
                Assert.Throws<ObjectDisposedException>(() => { stmt.ClearBindings(); });
                Assert.Throws<ObjectDisposedException>(() => { stmt.MoveNext(); });
                Assert.Throws<ObjectDisposedException>(() => { stmt.Reset(); });
                Assert.Throws<ObjectDisposedException>(() => { stmt.Status(StatementStatusCode.Sort, false); });
            }
        }

        [Fact]
        public void TestBusy()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                db.ExecuteAll(
                    @"CREATE TABLE foo (x int);
                      INSERT INTO foo (x) VALUES (1);
                      INSERT INTO foo (x) VALUES (2);
                      INSERT INTO foo (x) VALUES (3);");

                using (var stmt = db.PrepareStatement("SELECT x FROM foo;"))
                {
                    Assert.False(stmt.IsBusy);
                    stmt.MoveNext();
                    Assert.True(stmt.IsBusy);
                    stmt.MoveNext();
                    Assert.True(stmt.IsBusy);
                    stmt.MoveNext();
                    Assert.True(stmt.IsBusy);
                    stmt.MoveNext();
                    Assert.False(stmt.IsBusy);
                }
            }
        }

        [Fact]
        public void TestBindParameterCount()
        {
            Tuple<string, int>[] tests =
            {
                Tuple.Create("CREATE TABLE foo (x int)", 0),
                Tuple.Create("CREATE TABLE foo2 (x int, y int)", 0),
                Tuple.Create("select * from foo", 0),
                Tuple.Create("INSERT INTO foo (x) VALUES (?)", 1),
                Tuple.Create("INSERT INTO foo2 (x, y) VALUES (?, ?)", 2)
            };

            using (var db = SQLite3.OpenInMemory())
            {
                foreach (var test in tests)
                {
                    using (var stmt = db.PrepareStatement(test.Item1))
                    {
                        Assert.Equal(stmt.BindParameters.Count, test.Item2);
                        stmt.MoveNext();
                    }
                }
            }
        }

        [Fact]
        public void TestReadOnly()
        {
            Tuple<string, bool>[] tests =
            {
                Tuple.Create("CREATE TABLE foo (x int)", false),
                Tuple.Create("CREATE TABLE foo2 (x int, y int)", false),
                Tuple.Create("select * from foo", true),
                Tuple.Create("INSERT INTO foo (x) VALUES (?)", false),
                Tuple.Create("INSERT INTO foo2 (x, y) VALUES (?, ?)", false)
            };

            using (var db = SQLite3.OpenInMemory())
            {
                foreach (var test in tests)
                {
                    using (var stmt = db.PrepareStatement(test.Item1))
                    {
                        Assert.Equal(stmt.IsReadOnly, test.Item2);
                        stmt.MoveNext();
                    }
                }
            }
        }

        [Fact]
        public void TestGetSQL()
        {
            string[] sql =
            {
                "CREATE TABLE foo (x int)",
                "INSERT INTO foo (x) VALUES (1)",
                "INSERT INTO foo (x) VALUES (2)",
                "INSERT INTO foo (x) VALUES (3)",
                "SELECT x FROM foo",
            };

            using (var db = SQLite3.OpenInMemory())
            {
                foreach (var sqlStmt in sql)
                {
                    using (var stmt = db.PrepareStatement(sqlStmt))
                    {
                        stmt.MoveNext();

                        Assert.Equal(stmt.SQL, sqlStmt);
                    }
                }
            }
        }

        [Fact]
        public void TestGetBindParameters()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                db.Execute("CREATE TABLE foo (x int, v int, t text, d real, b blob, q blob);");

                using (var stmt = db.PrepareStatement("INSERT INTO foo (x,v,t,d,b,q) VALUES (:x,:v,:t,:d,:b,:q)"))
                {
                    Assert.Equal(":x", stmt.BindParameters[0].Name);
                    Assert.Equal(":v", stmt.BindParameters[1].Name);
                    Assert.Equal(":t", stmt.BindParameters[2].Name);
                    Assert.Equal(":d", stmt.BindParameters[3].Name);
                    Assert.Equal(":b", stmt.BindParameters[4].Name);
                    Assert.Equal(":q", stmt.BindParameters[5].Name);

                    Assert.Equal(":x", stmt.BindParameters[":x"].Name);
                    Assert.Equal(":v", stmt.BindParameters[":v"].Name);
                    Assert.Equal(":t", stmt.BindParameters[":t"].Name);
                    Assert.Equal(":d", stmt.BindParameters[":d"].Name);
                    Assert.Equal(":b", stmt.BindParameters[":b"].Name);
                    Assert.Equal(":q", stmt.BindParameters[":q"].Name);

                    Assert.True(stmt.BindParameters.ContainsKey(":x"));
                    Assert.False(stmt.BindParameters.ContainsKey(":nope"));
                    Assert.Equal(6, stmt.BindParameters.Keys.Count());
                    Assert.Equal(6, stmt.BindParameters.Values.Count());

                    Assert.Throws<KeyNotFoundException>(() => { var x = stmt.BindParameters[":nope"]; });
                    Assert.Throws<ArgumentOutOfRangeException>(() => { var x = stmt.BindParameters[-1]; });
                    Assert.Throws<ArgumentOutOfRangeException>(() => { var x = stmt.BindParameters[100]; });

                    Assert.NotNull(((IEnumerable) stmt.BindParameters).GetEnumerator());
                }
            }
        }

        [Fact]
        public void TestExecute()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                db.Execute("CREATE TABLE foo (v int);");
                using (var stmt = db.PrepareStatement("INSERT INTO foo (v) VALUES (?)"))
                {
                    foreach (var i in Enumerable.Range(0, 100))
                    {
                        stmt.Execute(i);
                    }
                }

                foreach (var result in db.Query("SELECT v FROM foo ORDER BY 1").Select((v, index) => Tuple.Create(index, v[0].ToInt())))
                {
                    Assert.Equal(result.Item2, result.Item1);
                }
            }
        }

        [Fact]
        public void TestQuery()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                db.Execute("CREATE TABLE foo (v int);");
                using (var stmt = db.PrepareStatement("INSERT INTO foo (v) VALUES (?)"))
                {
                    foreach (var i in Enumerable.Range(0, 100))
                    {
                        stmt.Execute(i);
                    }
                }

                using (var stmt = db.PrepareStatement("SELECT * from FOO WHERE v < ?"))
                {
                    var result = stmt.Query(50).Count();

                    // Ensure that enumerating the Query Enumerable doesn't dispose the stmt
                    { var x = stmt.IsBusy; }
                    Assert.Equal(50, result);
                }

                using (var stmt = db.PrepareStatement("SELECT * from FOO WHERE v < 50"))
                {
                    var result = stmt.Query().Count();

                    // Ensure that enumerating the Query Enumerable doesn't dispose the stmt
                    { var x = stmt.IsBusy; }
                    Assert.Equal(50, result);
                }
            }
        }

        [Fact]
        public void TestClearBindings()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                db.Execute("CREATE TABLE foo (x int, v int);");

                using (var stmt = db.PrepareStatement("INSERT INTO foo (x,v) VALUES (:x,:v)"))
                {
                    stmt.BindParameters[0].Bind(1);
                    stmt.BindParameters[1].Bind(2);
                    stmt.MoveNext();

                    stmt.Reset();
                    stmt.ClearBindings();
                    stmt.MoveNext();
                }

                var last =
                    db.Query("SELECT * from FOO")
                        .Select(row => Tuple.Create(row[0].ToInt(), row[1].ToInt()))
                        .Last();

                Assert.Equal(0, last.Item1);
                Assert.Equal(0, last.Item2);
            }
        }

        [Fact]
        public void TestGetColumns()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                var count = 0;
                var stmt = db.PrepareStatement("SELECT 1 as a, 2 as a, 3 as a");
                foreach (var column in stmt.Columns)
                {
                    count++;
                    Assert.Equal("a", column.Name);
                }

                Assert.Throws<ArgumentOutOfRangeException>(() => { var x = stmt.Columns[-1]; });
                Assert.Throws<ArgumentOutOfRangeException>(() => { var x = stmt.Columns[3]; });

                Assert.Equal(stmt.Columns.Count, count);
            }
        }

        [Fact]
        public void TestStatus()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                db.Execute("CREATE TABLE foo (x int);");

                using (var stmt = db.PrepareStatement("SELECT x FROM foo"))
                {
                    stmt.MoveNext();

                    int vmStep = stmt.Status(StatementStatusCode.VirtualMachineStep, false);
                    Assert.True(vmStep > 0);

                    int vmStep2 = stmt.Status(StatementStatusCode.VirtualMachineStep, true);
                    Assert.Equal(vmStep2, vmStep);

                    int vmStep3 = stmt.Status(StatementStatusCode.VirtualMachineStep, false);
                    Assert.Equal(0, vmStep3);
                }
            }
        }
    }
}
