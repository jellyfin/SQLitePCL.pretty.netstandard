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
using System.Linq;
using Xunit;

namespace SQLitePCL.pretty.Tests
{
    public class BlobStreamTests
    {
        static BlobStreamTests()
        {
            Batteries_V2.Init();
        }

        [Fact]
        public void TestRead()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                byte[] bytes = new byte[1000];
                Random random = new Random();
                random.NextBytes(bytes);

                db.Execute("CREATE TABLE foo (x blob);");
                db.Execute("INSERT INTO foo (x) VALUES(?);", bytes);

                using (var stream = db.Query("SELECT rowid, x FROM foo;")
                    .Select(row => db.OpenBlob(row[1].ColumnInfo, row[0].ToInt64(), false))
                    .First())
                {
                    Assert.True(stream.CanRead);
                    Assert.False(stream.CanWrite);
                    Assert.True(stream.CanSeek);

                    for (int i = 0; i < stream.Length; i++)
                    {
                        int b = stream.ReadByte();
                        Assert.Equal(b, bytes[i]);
                    }

                    // Since this is a read only stream, this is a good chance to test that writing fails
                    Assert.Throws<NotSupportedException>(() => stream.WriteByte(0));
                }
            }
        }

        [Fact]
        public void TestDispose()
        {
            Stream notDisposedStream;

            using (var db = SQLite3.OpenInMemory())
            {
                db.Execute("CREATE TABLE foo (x blob);");
                db.Execute("INSERT INTO foo (x) VALUES(?);", "data");
                var blob =
                    db.Query("SELECT rowid, x FROM foo")
                        .Select(row => db.OpenBlob(row[1].ColumnInfo, row[0].ToInt64(), true))
                        .First();
                blob.Dispose();

                // Test double dispose doesn't crash
                blob.Dispose();

                Assert.Throws<ObjectDisposedException>(() => { var x = blob.Length; });
                Assert.Throws<ObjectDisposedException>(() => { var x = blob.Position; });
                Assert.Throws<ObjectDisposedException>(() => { blob.Position = 10; });
                Assert.Throws<ObjectDisposedException>(() => { blob.Read(new byte[10], 0, 2); });
                Assert.Throws<ObjectDisposedException>(() => { blob.Write(new byte[10], 0, 1); });
                Assert.Throws<ObjectDisposedException>(() => { blob.Seek(0, SeekOrigin.Begin); });

                notDisposedStream =
                    db.Query("SELECT rowid, x FROM foo;")
                        .Select(row => db.OpenBlob(row[1].ColumnInfo, row[0].ToInt64(), false))
                        .First();
            }

            // Test that disposing the connection disposes the stream
            Assert.Throws<ObjectDisposedException>(() => { var x = notDisposedStream.Length; });
        }

        [Fact]
        public void TestSeek()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                db.Execute("CREATE TABLE foo (x blob);");
                db.Execute("INSERT INTO foo (x) VALUES(?);", "data");
                using (var blob = db.Query("SELECT rowid, x FROM foo")
                    .Select(row => db.OpenBlob(row[1].ColumnInfo, row[0].ToInt64(), true))
                    .First())
                {
                    Assert.True(blob.CanSeek);
                    Assert.Throws<NotSupportedException>(() => blob.SetLength(10));
                    { blob.Position = 100; }

                    // Test input validation
                    blob.Position = 5;
                    Assert.Throws<IOException>(() => blob.Seek(-10, SeekOrigin.Begin));
                    Assert.Equal(5, blob.Position);
                    Assert.Throws<IOException>(() => blob.Seek(-10, SeekOrigin.Current));
                    Assert.Equal(5, blob.Position);
                    Assert.Throws<IOException>(() => blob.Seek(-100, SeekOrigin.End));
                    Assert.Equal(5, blob.Position);
                    Assert.Throws<ArgumentException>(() => blob.Seek(-100, (SeekOrigin)10));
                    Assert.Equal(5, blob.Position);

                    blob.Seek(0, SeekOrigin.Begin);
                    Assert.Equal(0, blob.Position);

                    blob.Seek(0, SeekOrigin.End);
                    Assert.Equal(blob.Length, blob.Position);

                    blob.Position = 5;
                    blob.Seek(2, SeekOrigin.Current);
                    Assert.Equal(7, blob.Position);
                }
            }
        }

        [Fact]
        public void TestWrite()
        {
            using (var db = SQLite3.OpenInMemory())
            {
                byte[] bytes = new byte[1000];
                Random random = new Random();
                random.NextBytes(bytes);

                var source = new MemoryStream(bytes);

                db.Execute("CREATE TABLE foo (x blob);");
                db.Execute("INSERT INTO foo (x) VALUES(?);", source);

                using (var stream = db.Query("SELECT rowid, x FROM foo")
                    .Select(row => db.OpenBlob(row[1].ColumnInfo, row[0].ToInt64(), true))
                    .First())
                {
                    Assert.True(stream.CanRead);
                    Assert.True(stream.CanWrite);
                    source.CopyTo(stream);

                    stream.Position = 0;

                    for (int i = 0; i < stream.Length; i++)
                    {
                        int b = stream.ReadByte();
                        Assert.Equal(b, bytes[i]);
                    }

                    // Test writing after the end of the stream
                    // Assert that nothing changes.
                    stream.Position = stream.Length;
                    stream.Write(new byte[10], 0, 10);
                    stream.Position = 0;
                    for (int i = 0; i < stream.Length; i++)
                    {
                        int b = stream.ReadByte();
                        Assert.Equal(b, bytes[i]);
                    }
                }
            }
        }
    }
}
