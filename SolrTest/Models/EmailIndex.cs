using System;
using System.Collections.Generic;
using SolrNet.Attributes;

namespace SolrTest.Models
{
    public class EmailIndex
    {
        [SolrUniqueKey(Keys.Id)]
        public string Id { get; set; }

        [SolrField(Keys.DateSent)]
        public DateTime DateSent { get; set; }

        [SolrField(Keys.To)]
        public string To { get; set; }

        [SolrField(Keys.From)]
        public string From { get; set; }

        [SolrField(Keys.Subject)]
        public string Subject { get; set; }

        [SolrField(Keys.MessageBody)]
        public string MessageBody { get; set; }

        [SolrField(Keys.MatchedFunctions)]
        public ICollection<string> MatchedFunctions { get; set; }

        [SolrField(Keys.UnmatchedFunctions)]
        public ICollection<string> UnmatchedFunctions { get; set; }

        public class Keys
        {
            public const string Id = "id";
            public const string DateSent = "date_sent";
            public const string To = "to";
            public const string From = "from";
            public const string Subject = "subject";
            public const string MessageBody = "message_body";
            public const string MatchedFunctions = "matched_functions";
            public const string UnmatchedFunctions = "unmatched_functions";
        }
    }
}