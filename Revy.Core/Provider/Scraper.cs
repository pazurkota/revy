using AngleSharp;
using AngleSharp.Dom;
using Revy.Core.Model;

namespace Revy.Core.Provider;

public class Scraper
{
    private readonly IBrowsingContext _context;
    private readonly string _baseUrl = "https://gogoanime.by/";

    public Scraper()
    {
        var config = Configuration.Default.WithDefaultLoader();
        _context = BrowsingContext.New(config);
    }

    public async Task<List<AnimeSearchResult>> SearchAnimeAsync(string query)
    {
        var results = new List<AnimeSearchResult>();
        var searchUrl = $"{_baseUrl}/filter.html?keyword={Uri.EscapeDataString(query)}";

        IDocument document = await _context.OpenAsync(searchUrl);
        var elements = document.QuerySelectorAll(".last_episodes ul.items li");

        foreach (var element in elements)
        {
            var linkElement = element.QuerySelector(".name a");
            if (linkElement != null)
            {
                string title = linkElement.TextContent.Trim();
                string? relativeUrl = linkElement.GetAttribute("href");

                if (!string.IsNullOrEmpty(relativeUrl))
                {
                    results.Add(new AnimeSearchResult(title, _baseUrl + relativeUrl));
                }
            }
        }
        
        return results;
    }
}