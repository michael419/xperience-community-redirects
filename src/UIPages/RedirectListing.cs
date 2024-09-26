using System.Globalization;
using CMS.Base;
using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Websites;
using CMS.Websites.Internal;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.Xperience.Admin.Base;
using XperienceCommunity.Redirects.UIPages;

[assembly: UIPage(
    parentType: typeof(RedirectApplicationPage),
    slug: "list",
    uiPageType: typeof(RedirectListing),
    name: "List",
    templateName: TemplateNames.LISTING,
    order: UIPageOrder.First)]

namespace XperienceCommunity.Redirects.UIPages;

public class RedirectListing : ListingPage
{
    protected override string ObjectType => RedirectInfo.OBJECT_TYPE;

    private readonly IContentQueryExecutor _executor;
    private readonly IWebPageUrlRetriever _webPageUrlRetriever;
    
    public RedirectListing(IContentQueryExecutor executor, IWebPageUrlRetriever webPageUrlRetriever)
    {
        _executor = executor;
        _webPageUrlRetriever = webPageUrlRetriever;
    }

    public override async Task ConfigurePage()
    {
        PageConfiguration.HeaderActions.AddLink<RedirectCreate>("New redirect");
        PageConfiguration.TableActions.AddDeleteAction(nameof(Delete));
        PageConfiguration.AddEditRowAction<RedirectEdit>();

        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(RedirectInfo.RedirectSourceUrl), "Source URL", searchable: true)
            .AddColumn(nameof(RedirectInfo.RedirectTargetWebPageItemGUID), "Target web page", formatter: GetWebPageUrl);

        PageConfiguration.AddEditRowAction<RedirectEditSection>();
        
        await base.ConfigurePage();
    }

    [PageCommand(Permission = SystemPermissions.DELETE)]
    public override Task<ICommandResponse<RowActionResult>> Delete(int id) => base.Delete(id);
    
    private string GetWebPageUrl(object objectValue, IDataContainer dataContainer)
    {
        Guid webPageItemGuid = ValidationHelper.GetGuid(objectValue, Guid.Empty);
        
        if (webPageItemGuid != Guid.Empty)
        {
            var pageQuery = new ContentItemQueryBuilder()
                .ForContentTypes(parameters =>
                    parameters.ForWebsite([webPageItemGuid], includeUrlPath: false)
                )
                .Parameters(parameters => 
                    parameters
                        .Columns(nameof(WebPageItemInfo.WebPageItemID))
                        .TopN(1)
                );

            var result = _executor.GetMappedResult<IWebPageFieldsSource>(pageQuery).Result.FirstOrDefault();

            if (result != null)
            {
                string pageUrl = _webPageUrlRetriever.Retrieve(result.SystemFields.WebPageItemID, GetDefaultLanguageName()).Result.RelativePath;

                if (pageUrl != null)
                {
                    return pageUrl.Replace("~", "");
                }
            }
        }

        return "Page deleted";
    }

    private string GetDefaultLanguageName()
    {
        return ContentLanguageInfo.Provider.Get()
            .TopN(1)
            .Column(nameof(ContentLanguageInfo.ContentLanguageName))
            .WhereTrue(nameof(ContentLanguageInfo.ContentLanguageIsDefault))
            .GetScalarResult<string>();
    }
}

