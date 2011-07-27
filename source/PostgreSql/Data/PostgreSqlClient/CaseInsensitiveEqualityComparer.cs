using System.Collections.Generic;
using System.Globalization;

namespace PostgreSql.Data.PostgreSqlClient
{
    internal sealed class CaseInsensitiveEqualityComparer
        : EqualityComparer<string>
    {
        #region · Overriden Methods ·

        public override bool Equals(string x, string y)
        {
            return CultureInfo.CurrentCulture.CompareInfo.Compare
            (
                x,
                y,
                CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth |
                CompareOptions.IgnoreCase
            ) == 0 ? true : false;
        }

        public override int GetHashCode(string obj)
        {
            return obj.ToLowerInvariant().GetHashCode();
        }

        #endregion
    }
}
