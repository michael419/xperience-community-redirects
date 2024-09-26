using CMS.ContentEngine;
using CMS.Websites;
using CMS.Websites.Internal;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using XperienceCommunity.Redirects.Services;

namespace XperienceCommunity.Redirects;

public class RedirectMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRedirectService _redirectService;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    
    private string[] ExcludedStartingPaths = new []
    {
        "/",
        "/cmsctx",
        "/admin",
        "/getmedia",
        "/getcontentasset",
        "/kentico."
    };

    public RedirectMiddleware(
        RequestDelegate next,
        IRedirectService redirectService,
        IWebPageUrlRetriever webPageUrlRetriever
        )
    {
        _webPageUrlRetriever = webPageUrlRetriever;
        _next = next;
        _redirectService = redirectService;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        string requestPath = context.Request.Path.Value?.ToLower() ?? string.Empty;

        foreach (string excludedPath in ExcludedStartingPaths)
        {
            if (context.Request.Path.StartsWithSegments(excludedPath, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
            }
        }
        
        List<RedirectInfo> allRedirects = (await _redirectService.GetRedirects()).ToList();

        if (allRedirects == null || allRedirects.Count == 0)
        {
            await _next(context);
        }
        
        RedirectInfo matchingRedirectInfo = allRedirects.FirstOrDefault(r => r.RedirectSourceUrl == requestPath);

        if (matchingRedirectInfo != null)
        {
            int? targetWebPageItemId = GetWebPageItemId(matchingRedirectInfo.RedirectTargetWebPageItemGUID);

            if (targetWebPageItemId.HasValue)
            {
                var languages = GetContentLangauges().ToList();

                string firstSegment = context.Request.Path.ToString().Split('/').First();

                if (context.Request.Path.ToString().Split('/').Length > 1)
                {
                    firstSegment = context.Request.Path.ToString().Split('/')[1];
                }

                var currentLanguage = languages.First(l => l.ContentLanguageIsDefault);
                
                if (!string.IsNullOrEmpty(firstSegment))
                {
                    firstSegment = firstSegment?.ToLower() ?? string.Empty;
                    
                    var matchedLanguage = languages.FirstOrDefault(l => 
                        l.ContentLanguageName.Equals(firstSegment, StringComparison.CurrentCultureIgnoreCase)
                        || l.ContentLanguageCultureFormat.Equals(firstSegment, StringComparison.CurrentCultureIgnoreCase));

                    if (matchedLanguage != null)
                    {
                        currentLanguage = matchedLanguage;
                    }
                }
                
                string targetPageUrl = _webPageUrlRetriever.Retrieve(targetWebPageItemId.Value, currentLanguage?.ContentLanguageName).Result.RelativePath.Replace("~", "");

                if (!requestPath.Equals(targetPageUrl, StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.Redirect(targetPageUrl, permanent: true);
                
                    await context.Response.CompleteAsync();
                    
                    return;
                }
            }
        }

        await _next(context);
    }

    private int? GetWebPageItemId(Guid webPageItemGuid)
    {
        return WebPageItemInfo.Provider.Get()
            .TopN(1)
            .Column(nameof(WebPageItemInfo.WebPageItemID))
            .WhereEquals(nameof(WebPageItemInfo.WebPageItemGUID), webPageItemGuid)
            .GetScalarResult<int>();
    }
    
    private IEnumerable<ContentLanguageInfo> GetContentLangauges()
    {
        return ContentLanguageInfo.Provider.Get()
            .Columns(nameof(ContentLanguageInfo.ContentLanguageName),
                nameof(ContentLanguageInfo.ContentLanguageCultureFormat),
                nameof(ContentLanguageInfo.ContentLanguageIsDefault))
            .ToList();
    }
}

public static class RedirectMiddlewareExtensions
{
    public static IApplicationBuilder UseXperienceCommunityRedirects(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RedirectMiddleware>();
    }
}
