using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;

namespace XperienceCommunity.Redirects.Services
{
    public interface IRedirectService
    {
        Task<IReadOnlyCollection<RedirectInfo>> GetRedirects();
    }

    public class RedirectService : IRedirectService
    {
        private readonly IInfoProvider<RedirectInfo> _redirectInfoProvider;
        private readonly IProgressiveCache _cache;

        public RedirectService(IInfoProvider<RedirectInfo> redirectInfoProvider, IProgressiveCache cache)
        {
            _redirectInfoProvider = redirectInfoProvider;
            _cache = cache;
        }

        public async Task<IReadOnlyCollection<RedirectInfo>> GetRedirects()
        {
            return await _cache.LoadAsync(async s =>
            {
                s.GetCacheDependency = () =>
                    CacheHelper.GetCacheDependency(
                        [
                            $"{RedirectInfo.OBJECT_TYPE}|all"
                        ]);

                var redirects = await _redirectInfoProvider.Get()
                    .GetEnumerableTypedResultAsync()
                    .ConfigureAwait(false);

                return redirects.ToList();

            }, new CacheSettings(CacheHelper.CacheMinutes(), $"{nameof(RedirectService)}.{nameof(GetRedirects)}", "all"));
        }
    }
}