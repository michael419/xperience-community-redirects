using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using XperienceCommunity.Redirects.Admin;
using XperienceCommunity.Redirects.UIPages;

[assembly: UIApplication(
    identifier: RedirectApplicationPage.IDENTIFIER,
    type: typeof(RedirectApplicationPage),
    slug: "redirects",
    name: "Redirects",
    category: BaseApplicationCategories.CONTENT_MANAGEMENT,
    icon: Icons.ArrowURight,
    templateName: TemplateNames.SECTION_LAYOUT)]

namespace XperienceCommunity.Redirects.UIPages;

[UIPermission(SystemPermissions.VIEW)]
[UIPermission(SystemPermissions.CREATE)]
[UIPermission(SystemPermissions.DELETE)]
[UIPermission(SystemPermissions.UPDATE)]
internal class RedirectApplicationPage : ApplicationPage
{
    public const string IDENTIFIER = "redirects";
}
