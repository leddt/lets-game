using System.Collections.Generic;

namespace LetsGame.Web.Services.Itad.Model
{
    public class ItadSearchResults
    {
        public ItadSearchResult[] Results { get; set; } = new ItadSearchResult[0];
        public IDictionary<string, string> Urls { get; set; } = new Dictionary<string, string>();
    }
}