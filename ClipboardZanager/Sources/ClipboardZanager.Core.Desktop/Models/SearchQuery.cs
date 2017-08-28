using System;
using System.Linq;
using System.Text.RegularExpressions;
using ClipboardZanager.Core.Desktop.Enums;

namespace ClipboardZanager.Core.Desktop.Models
{
    /// <summary>
    /// Represents an analyze of the query that the user typed in the software to perform a search
    /// </summary>
    internal sealed class SearchQuery
    {
        /// <summary>
        /// Gets the query typed by the user
        /// </summary>
        internal string Query { get; }

        /// <summary>
        /// Gets the query typed by the user
        /// </summary>
        internal Regex QueryRegex { get; }

        /// <summary>
        /// Gets a value that defines when to perform the search.
        /// </summary>
        internal SearchType Type { get; }

        /// <summary>
        /// Initialize a new instance of the <see cref="SearchQuery"/> class.
        /// </summary>
        /// <param name="input">The raw input typed by the user</param>
        /// <param name="searchType">The type of data to search</param>
        internal SearchQuery(string input, SearchType searchType)
        {
            Type = searchType;

            if (string.IsNullOrEmpty(input))
            {
                input = string.Empty;
            }

            if (Type != SearchType.All)
            {
                if (!input.All(char.IsWhiteSpace))
                {
                    input = input.Trim();
                }
            }

            Query = input;

            try
            {
                QueryRegex = new Regex(input.Replace(".", "\\.").Replace("*", "."));
            }
            catch
            {
                QueryRegex = new Regex(".");
            }
        }
    }
}
