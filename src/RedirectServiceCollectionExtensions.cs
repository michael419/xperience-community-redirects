using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.Redirects.Admin;
using XperienceCommunity.Redirects.Services;

namespace XperienceCommunity.Redirects;

public static class RedirectServiceCollectionExtensions
{
    /// <summary>
    /// Adds all required services for Redirect functionality
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddXperienceCommunityRedirects(this IServiceCollection services)
    {
        services.AddSingleton<IRedirectModuleInstaller, RedirectModuleInstaller>();
        services.AddSingleton<IRedirectService, RedirectService>();

        return services;
    }
}
