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
using System.Linq;
using Xunit;

namespace SQLitePCL.pretty.Tests
{
    public class DatabaseBackupTests
    {
        static DatabaseBackupTests()
        {
            Batteries_V2.Init();
        }

        [Fact]
        public void TestDispose()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                db.Execute("CREATE TABLE foo (x int);");
                foreach (int i in Enumerable.Range(0, 1000))
                {
                    db.Execute("INSERT INTO foo (x) VALUES (?);", i);
                }

                IDatabaseBackup notDisposedBackup;

                using (var db2 = SQLite3.OpenInMemory())
                {
                    var backup = db.BackupInit("main", db2, "main");
                    backup.Dispose();

                    Assert.Throws<ObjectDisposedException>(() => { var x = backup.PageCount; });
                    Assert.Throws<ObjectDisposedException>(() => { var x = backup.RemainingPages; });
                    Assert.Throws<ObjectDisposedException>(() => { backup.Step(1); });

                    notDisposedBackup = db.BackupInit("main", db2, "main");
                }

                // Ensure diposing the database connection automatically disposes the backup as well.
                Assert.Throws<ObjectDisposedException>(() => { var x = notDisposedBackup.PageCount; });

                // Test double disposing doesn't result in exceptions.
                notDisposedBackup.Dispose();
            }
        }

        [Fact]
        public void TestBackupWithPageStepping()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                db.Execute("CREATE TABLE foo (x int);");
                foreach (int i in Enumerable.Range(0, 1000))
                {
                    db.Execute("INSERT INTO foo (x) VALUES (?);", i);
                }

                using (var db2 = SQLite3.OpenInMemory())
                {
                    using (var backup = db.BackupInit("main", db2, "main"))
                    {
                        Assert.Equal(0, backup.RemainingPages);
                        Assert.Equal(0, backup.PageCount);

                        backup.Step(1);
                        var remainingPages = backup.RemainingPages;

                        while (backup.Step(1))
                        {
                            Assert.True(backup.RemainingPages < remainingPages);
                            remainingPages = backup.RemainingPages;
                        }

                        Assert.False(backup.Step(2));
                        Assert.False(backup.Step(-1));
                        Assert.Equal(0, backup.RemainingPages);
                        Assert.True(backup.PageCount > 0);
                    }
                }
            }
        }

        [Fact]
        public void TestBackup()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                db.Execute("CREATE TABLE foo (x int);");
                foreach (int i in Enumerable.Range(0, 1000))
                {
                    db.Execute("INSERT INTO foo (x) VALUES (?);", i);
                }

                using (var db2 = SQLite3.OpenInMemory())
                {
                    using (var backup = db.BackupInit("main", db2, "main"))
                    {
                        Assert.Equal(0, backup.RemainingPages);
                        Assert.Equal(0, backup.PageCount);

                        Assert.False(backup.Step(-1));

                        Assert.Equal(0, backup.RemainingPages);
                        Assert.True(backup.PageCount > 0);
                    }
                }

                using (var db3 = SQLite3.OpenInMemory())
                {
                    db.Backup("main", db3, "main");
                    var backupResults = Enumerable.Zip(
                        db.Query("SELECT x FROM foo"),
                        db3.Query("SELECT x FROM foo"),
                        Tuple.Create);

                    foreach (var pair in backupResults)
                    {
                        Assert.Equal(pair.Item2[0].ToInt(), pair.Item1[0].ToInt());
                    }
                }
            }
        }
    }
}
