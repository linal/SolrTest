using System.Collections;
using System.Collections.Generic;
using SolrNet.Attributes;

namespace SolrTest.Models
{
    public class FileIndex
    {
        [SolrUniqueKey(Keys.Id)]
        public string Id { get; set; }

        [SolrField(Keys.FilePath)]
        public string FilePath { get; set; }

        [SolrField(Keys.Delete)]
        public bool Delete { get; set; }

        [SolrField(Keys.MatchedFunctions)]
        public ICollection<string> MatchedFunctions { get; set; }

        [SolrField(Keys.UnmatchedFunctions)]
        public ICollection<string> UnmatchedFunctions { get; set; }

        public FileIndex()
        {
            MatchedFunctions = new List<string>();
            UnmatchedFunctions = new List<string>();
        }

        public class Keys
        {
            public const string Id = "id";
            public const string FilePath = "file_path";
            public const string Delete = "delete";
            public const string MatchedFunctions = "matched_functions";
            public const string UnmatchedFunctions = "unmatched_functions";
        }
    }
}