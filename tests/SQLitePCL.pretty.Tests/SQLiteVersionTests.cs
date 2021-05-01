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

using Xunit;
using System;

namespace SQLitePCL.pretty.Tests
{
    public class SQLiteVersionTests
    {
        [Fact]
        public void TestEquality()
        {
            Assert.True(SQLiteVersion.Of(3080911).Equals(SQLiteVersion.Of(3080911)));
            Assert.True(SQLiteVersion.Of(3080911).Equals((object)SQLiteVersion.Of(3080911)));
            Assert.True(SQLiteVersion.Of(3080911) == SQLiteVersion.Of(3080911));
            Assert.False(SQLiteVersion.Of(3080911) != SQLiteVersion.Of(3080911));

            SQLiteVersion[] notEqualTests =
            {
                SQLiteVersion.Of(3080911),
                SQLiteVersion.Of(2080911),
                SQLiteVersion.Of(3070911),
                SQLiteVersion.Of(3080910)
            };

            for (int i = 0; i < notEqualTests.Length; i++)
            {
                for (int j = i + 1; j < notEqualTests.Length; j++)
                {
                    Assert.False(notEqualTests[i].Equals(notEqualTests[j]));
                    Assert.False(notEqualTests[i].Equals((object)notEqualTests[j]));
                    Assert.False(notEqualTests[i] == notEqualTests[j]);
                    Assert.True(notEqualTests[i] != notEqualTests[j]);
                }
            }

            Assert.False(SQLiteVersion.Of(3080911).Equals(null));
            Assert.False(SQLiteVersion.Of(3080911).Equals(""));
        }

        [Fact]
        public void TestGetHashcode()
        {
            SQLiteVersion[] equalObjects =
            {
                SQLiteVersion.Of(3080911),
                SQLiteVersion.Of(3080911),
                SQLiteVersion.Of(3080911)
            };

            for (int i = 0; i < equalObjects.Length; i++)
            {
                for (int j = i + 1; j < equalObjects.Length; j++)
                {
                    Assert.Equal(equalObjects[j].GetHashCode(), equalObjects[i].GetHashCode());
                }
            }
        }

        [Fact]
        public void TestComparison()
        {
            Assert.Equal(0, SQLiteVersion.Of(3080911).CompareTo(SQLiteVersion.Of(3080911)));

            Assert.True(SQLiteVersion.Of(3080911) < SQLiteVersion.Of(3080912));
            Assert.True(SQLiteVersion.Of(3080911) < SQLiteVersion.Of(3081911));
            Assert.True(SQLiteVersion.Of(3080911) < SQLiteVersion.Of(4080911));

            Assert.Throws<ArgumentException>(() => SQLiteVersion.Of(3080911).CompareTo(null));
            Assert.Throws<ArgumentException>(() => SQLiteVersion.Of(3080911).CompareTo(""));

            Assert.True(SQLiteVersion.Of(3080911) > SQLiteVersion.Of(3080910));
            Assert.True(SQLiteVersion.Of(3080911) >= SQLiteVersion.Of(3080911));
            Assert.True(SQLiteVersion.Of(3080911) >= SQLiteVersion.Of(3080910));

            Assert.False(SQLiteVersion.Of(3080911) < SQLiteVersion.Of(3080910));
            Assert.True(SQLiteVersion.Of(3080911) <= SQLiteVersion.Of(3080911));
            Assert.False(SQLiteVersion.Of(3080911) < SQLiteVersion.Of(3080910));
        }

        [Fact]
        public void TestToInt()
        {
            Assert.Equal(3080911, SQLiteVersion.Of(3080911).ToInt());
        }

        [Theory]
        [InlineData(3008007, "3.8.7")]
        [InlineData(44008007, "44.8.7")]
        [InlineData(3008080, "3.8.80")]
        [InlineData(3088007, "3.88.7")]
        public void TestToString(int value, string expected)
        {
            Assert.Equal(expected, SQLiteVersion.Of(value).ToString());
        }
    }
}
