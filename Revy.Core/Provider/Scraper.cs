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

    public async Task<List<EpisodeResult>> GetEpisodesAsync(string animeUrl)
    {
        var episodes = new List<EpisodeResult>();
        
        string animeAlias = animeUrl.Split("/").Last();
        var episodesUrl = $"https://ajax.gogo-load.com/ajax/load-list-episode?ep_start=0&ep_end=500&id=0&default_ep=0&alias={animeAlias}";

        IDocument document = await _context.OpenAsync(episodesUrl);

        var episodeLinks = document.QuerySelectorAll("#episode_related li a");

        foreach (var episodeLink in episodeLinks)
        {
            string name = episodeLink.QuerySelector(".name")?.TextContent.Trim() ?? "Episode";
            string? relativeUrl = episodeLink.GetAttribute("href");

            if (!string.IsNullOrEmpty(relativeUrl))
            {
                episodes.Add(new EpisodeResult(name, _baseUrl + relativeUrl.Trim()));
            }
        }
        
        return episodes;
    }

    public async Task<string?> GetIframeUrlAsync(string episodeUrl)
    {
        IDocument document = await _context.OpenAsync(episodeUrl);

        var iframe = document.QuerySelector(".play-video iframe");
        string? iframeUrl = iframe?.GetAttribute("src");

        if (string.IsNullOrEmpty(iframeUrl)) return null;
        if (iframeUrl.StartsWith("//")) iframeUrl = "https:" + iframeUrl;
        
        return iframeUrl;
    }
}