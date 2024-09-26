using Kentico.Xperience.Admin.Base;
using XperienceCommunity.Redirects;
using XperienceCommunity.Redirects.UIPages;

[assembly: UIPage(
    parentType: typeof(RedirectListing),
    slug: PageParameterConstants.PARAMETERIZED_SLUG,
    uiPageType: typeof(RedirectEditSection),
    name: "Edit section for Redirect configurations",
    templateName: TemplateNames.SECTION_LAYOUT,
    order: 300)]

namespace XperienceCommunity.Redirects.UIPages;

public class RedirectEditSection : EditSectionPage<RedirectInfo>
{
}