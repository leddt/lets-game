using System.Collections.Generic;

namespace LetsGame.Web.Services.Itad.Model
{
    public class ItadSearchResults
    {
        public ItadSearchResult[] Results { get; set; }
        public IDictionary<string, string> Urls { get; set; }
    }
}