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

namespace SQLitePCL.pretty
{
    /// <summary>
    /// Represents information about a single column in <see cref="IStatement"/> result set.
    /// </summary>
    public sealed class ColumnInfo : IEquatable<ColumnInfo>, IComparable<ColumnInfo>, IComparable
    {
        /// <summary>
        /// Indicates whether the two ColumnInfo instances are equal to each other.
        /// </summary>
        /// <param name="x">A ColumnInfo instance.</param>
        /// <param name="y">A ColumnInfo instance.</param>
        /// <returns><see langword="true"/> if the two instances are equal to each other; otherwise,  <see langword="false"/>.</returns>
        public static bool operator ==(ColumnInfo x, ColumnInfo y) =>
            object.ReferenceEquals(x, null) ? object.ReferenceEquals(y, null) : x.Equals(y);

        /// <summary>
        /// Indicates whether the two ColumnInfo instances are not equal each other.
        /// </summary>
        /// <param name="x">A ColumnInfo instance.</param>
        /// <param name="y">A ColumnInfo instance.</param>
        /// <returns><see langword="true"/> if the two instances are not equal to each other; otherwise,  <see langword="false"/>.</returns>
        public static bool operator !=(ColumnInfo x, ColumnInfo y) =>
            !(x == y);

        /// <summary>
        /// Indicates if the the first ColumnInfo is greater than or equal to the second.
        /// </summary>
        /// <param name="x">A ColumnInfo instance.</param>
        /// <param name="y">A ColumnInfo instance.</param>
        /// <returns><see langword="true"/> if the the first ColumnInfo is greater than or equal to the second; otherwise, <see langword="false"/>.</returns>
        public static bool operator >=(ColumnInfo x, ColumnInfo y) =>
            x.CompareTo(y) >= 0;

        /// <summary>
        /// Indicates if the the first ColumnInfo is greater than the second.
        /// </summary>
        /// <param name="x">A ColumnInfo instance.</param>
        /// <param name="y">A ColumnInfo instance.</param>
        /// <returns><see langword="true"/> if the the first ColumnInfo is greater than the second; otherwise, <see langword="false"/>.</returns>
        public static bool operator >(ColumnInfo x, ColumnInfo y) =>
            x.CompareTo(y) > 0;

        /// <summary>
        /// Indicates if the the first ColumnInfo is less than or equal to the second.
        /// </summary>
        /// <param name="x">A ColumnInfo instance.</param>
        /// <param name="y">A ColumnInfo instance.</param>
        /// <returns><see langword="true"/> if the the first ColumnInfo is less than or equal to the second; otherwise, <see langword="false"/>.</returns>
        public static bool operator <=(ColumnInfo x, ColumnInfo y) =>
            x.CompareTo(y) <= 0;

        /// <summary>
        /// Indicates if the the first ColumnInfo is less than the second.
        /// </summary>
        /// <param name="x">A ColumnInfo instance.</param>
        /// <param name="y">A ColumnInfo instance.</param>
        /// <returns><see langword="true"/> if the the first ColumnInfo is less than the second; otherwise, <see langword="false"/>.</returns>
        public static bool operator <(ColumnInfo x, ColumnInfo y) =>
            x.CompareTo(y) < 0;

        private static T TryOrDefault<T>(Func<T> f, T defaultValue)
        {
            try
            {
                return f();
            }
            catch
            {
                return defaultValue;
            }
        }

        internal static ColumnInfo Create(StatementImpl stmt, int index) =>
            new ColumnInfo(
                raw.sqlite3_column_name(stmt.sqlite3_stmt, index).utf8_to_string(),
                TryOrDefault(
                    () => raw.sqlite3_column_database_name(stmt.sqlite3_stmt, index).utf8_to_string(),
                    string.Empty),
                TryOrDefault(
                    () => raw.sqlite3_column_origin_name(stmt.sqlite3_stmt, index).utf8_to_string(),
                    string.Empty),
                TryOrDefault(
                    () => raw.sqlite3_column_table_name(stmt.sqlite3_stmt, index).utf8_to_string(),
                    string.Empty),
                raw.sqlite3_column_decltype(stmt.sqlite3_stmt, index).utf8_to_string());

        /// <summary>
        /// The column name.
        /// </summary>
        /// <seealso href="https://sqlite.org/c3ref/column_name.html"/>
        public string Name { get; }

        /// <summary>
        /// The database that is the origin of this particular result column.
        /// </summary>
        /// <seealso href="https://sqlite.org/c3ref/column_database_name.html"/>
        public string DatabaseName { get; }

        /// <summary>
        ///  The table that is the origin of this particular result column.
        /// </summary>
        /// <seealso href="https://sqlite.org/c3ref/column_database_name.html"/>
        public string TableName { get; }

        /// <summary>
        /// The column that is the origin of this particular result column.
        /// </summary>
        /// <seealso href="https://sqlite.org/c3ref/column_database_name.html"/>
        public string OriginName { get; }
       
        /// <summary>
        /// Returns the declared type of a column in a result set or null if no type is declared.
        /// </summary>
        /// <seealso href="https://sqlite.org/c3ref/column_decltype.html"/>
        public string DeclaredType { get; }

        internal ColumnInfo(string name, string databaseName, string originName, string tableName, string declaredType)
        {
            this.Name = name;
            this.DatabaseName = databaseName;
            this.OriginName = originName;
            this.TableName = tableName;
            this.DeclaredType = declaredType;
        }

        /// <inheritdoc/>
        public bool Equals(ColumnInfo other)
        {
            if (Object.ReferenceEquals(other, null))
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Name == other.Name &&
                this.DatabaseName == other.DatabaseName &&
                this.OriginName == other.OriginName &&
                this.TableName == other.TableName &&
                this.DeclaredType == other.DeclaredType;
        }

        /// <inheritdoc/>
        public override bool Equals(object other) =>
            other is ColumnInfo culumnInfo && this == culumnInfo;

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + this.Name.GetHashCode();
            hash = hash * 31 + this.DatabaseName.GetHashCode();
            hash = hash * 31 + this.OriginName.GetHashCode();
            hash = hash * 31 + this.TableName.GetHashCode();
            hash = hash * 32 + this.DeclaredType.GetHashCode();
            return hash;
        }

        /// <inheritdoc/>
        public int CompareTo(ColumnInfo other)
        {
            if (!Object.ReferenceEquals(other, null))
            {
                var result = String.CompareOrdinal(this.Name, other.Name);
                if (result != 0) { return result; }

                result = String.CompareOrdinal(this.DatabaseName, other.DatabaseName);
                if (result != 0) { return result; }

                result = String.CompareOrdinal(this.TableName, other.TableName);
                if (result != 0) { return result; }

                result = String.CompareOrdinal(this.OriginName, other.OriginName);
                if (result != 0) { return result; }

                return String.CompareOrdinal(this.DeclaredType, other.DeclaredType);
            }
            else
            {
                return 1;
            }
        }

        /// <inheritdoc/>>
        int IComparable.CompareTo(object obj) =>
            System.Object.ReferenceEquals(obj, null) ? 1 : this.CompareTo((ColumnInfo)obj);
    }
}
