using System;
using Xunit;

namespace SQLitePCL.pretty.Tests
{
    public class TableColumnMetadataTests
    {
        [Fact]
        public void TestEquality()
        {
            Tuple<string, string, bool, bool, bool>[] tests =
                {
                    Tuple.Create("", "", false, false, false),
                    Tuple.Create("a", "", false, false, false),
                    Tuple.Create("a", "b", false, false, false),
                    Tuple.Create("a", "b", true, false, false),
                    Tuple.Create("a", "b", true, true, false),
                    Tuple.Create("a", "b", true, true, true),
                };

            Assert.False(new TableColumnMetadata("", "", false, false, false).Equals(null));
            Assert.False(new TableColumnMetadata("", "", false, false, false).Equals(new object()));

            for (int i = 0; i < tests.Length; i++)
            {
                for (int j = 0; j < tests.Length; j++)
                {
                    var fst = new TableColumnMetadata(tests[i].Item1, tests[i].Item2, tests[i].Item3, tests[i].Item4, tests[i].Item5);
                    var snd = new TableColumnMetadata(tests[j].Item1, tests[j].Item2, tests[j].Item3, tests[j].Item4, tests[j].Item5);

                    if (i == j)
                    {
                        Assert.True(fst.Equals(fst));
                        Assert.True(snd.Equals(snd));
                        Assert.Equal(snd, fst);
                        Assert.True(fst == snd);
                        Assert.False(fst != snd);
                    }
                    else
                    {
                        Assert.NotEqual(fst, snd);
                        Assert.False(fst == snd);
                        Assert.True(fst != snd);
                    }
                }
            }
        }

        [Theory]
        [InlineData("", "", false, false, false)]
        [InlineData("a", "", false, false, false)]
        [InlineData("a", "b", false, false, false)]
        [InlineData("a", "b", true, false, false)]
        [InlineData("a", "b", true, true, false)]
        [InlineData("a", "b", true, true, true)]
        public void TestGetHashcode(string item1, string item2, bool item3, bool item4, bool item5)
        {

            var fst = new TableColumnMetadata(item1, item2, item3, item4, item5);
            var snd = new TableColumnMetadata(item1, item2, item3, item4, item5);

            Assert.Equal(snd.GetHashCode(), fst.GetHashCode());
        }

        [Fact]
        public void TestComparison()
        {
            TableColumnMetadata[] tests =
                {
                    new TableColumnMetadata("", "", false, false, false),
                    new TableColumnMetadata("a", "", false, false, false),
                    new TableColumnMetadata("a", "b", false, false, false),
                    new TableColumnMetadata("a", "b", true, false, false),
                    new TableColumnMetadata("a", "b", true, true, false),
                    new TableColumnMetadata("a", "b", true, true, true),
                };

            for (int i = 0; i < tests.Length; i++)
            {
                for (int j = 0; j < tests.Length; j++)
                {
                    if (i < j)
                    {
                        Assert.True(tests[i].CompareTo(tests[j]) < 0);
                        Assert.True(((IComparable)tests[i]).CompareTo(tests[j]) < 0);

                        Assert.True(tests[i] < tests[j]);
                        Assert.True(tests[i] <= tests[j]);
                        Assert.False(tests[i] > tests[j]);
                        Assert.False(tests[i] >= tests[j]);
                    }
                    else if (i == j)
                    {
                        Assert.Equal(0, tests[i].CompareTo(tests[j]));
                        Assert.Equal(0, ((IComparable)tests[i]).CompareTo(tests[j]));

                        Assert.True(tests[i] >= tests[j]);
                        Assert.True(tests[i] <= tests[j]);
                        Assert.False(tests[i] > tests[j]);
                        Assert.False(tests[i] < tests[j]);
                    }
                    else
                    {
                        Assert.True(tests[i].CompareTo(tests[j]) > 0);
                        Assert.True(((IComparable)tests[i]).CompareTo(tests[j]) > 0);

                        Assert.True(tests[i] > tests[j]);
                        Assert.True(tests[i] >= tests[j]);
                        Assert.False(tests[i] < tests[j]);
                        Assert.False(tests[i] <= tests[j]);
                    }
                }
            }

            TableColumnMetadata nullColumnInfo = null;
            Assert.Equal(1, new TableColumnMetadata("", "", false, false, false).CompareTo(nullColumnInfo));

            object nullObj = null;
            Assert.Equal(1, ((IComparable)new TableColumnMetadata("", "", false, false, false)).CompareTo(nullObj));
        }
    }
}
