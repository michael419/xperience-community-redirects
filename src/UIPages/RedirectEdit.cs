using CMS.DataEngine;
using CMS.Websites;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using XperienceCommunity.Redirects.UIPages;
using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

[assembly: UIPage(
    parentType: typeof(RedirectEditSection),
    slug: "edit",
    uiPageType: typeof(RedirectEdit),
    name: "Edit a redirect",
    templateName: TemplateNames.EDIT,
    order: UIPageOrder.First)]

namespace XperienceCommunity.Redirects.UIPages;

internal class RedirectEdit : ModelEditPage<RedirectEditModel>
{
    private RedirectEditModel? _model;
    private readonly IInfoProvider<RedirectInfo> _redirectInfoProvider;

    public RedirectEdit(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        IInfoProvider<RedirectInfo> redirectInfoProvider)
        : base(formItemCollectionProvider, formDataBinder)
    {
        _redirectInfoProvider = redirectInfoProvider;
    }

    protected override RedirectEditModel Model
    {
        get
        {
            if (_model != null)
            {
                return _model;
            }

            var info = _redirectInfoProvider.Get(ObjectID);
            if (info == null)
            {
                return new RedirectEditModel();
            }

            List<WebPageRelatedItem> targetWebPageItems = new List<WebPageRelatedItem>()
            {
                new() { WebPageGuid = info.RedirectTargetWebPageItemGUID }
            };
            
            _model = new RedirectEditModel()
            {
                TargetWebPageItem = targetWebPageItems,
                SourceUrl = info.RedirectSourceUrl
            };

            return _model;
        }
    }

    [PageParameter(typeof(IntPageModelBinder))]
    public int ObjectID { get; set; }

    protected override async Task<ICommandResponse> ProcessFormData(RedirectEditModel model, ICollection<IFormItem> formItems)
    {
        var info = _redirectInfoProvider.Get(ObjectID);

        model.MapToRedirectInfo(info);

        _redirectInfoProvider.Set(info);

        return await base.ProcessFormData(model, formItems);
    }
}
