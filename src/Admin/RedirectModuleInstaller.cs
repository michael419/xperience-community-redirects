using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Websites;
using static XperienceCommunity.Redirects.Admin.RedirectConstants;

namespace XperienceCommunity.Redirects.Admin;

internal interface IRedirectModuleInstaller
{
    void Install();
}

internal class RedirectModuleInstaller(IInfoProvider<ResourceInfo> resourceInfoProvider) : IRedirectModuleInstaller
{
    public void Install()
    {
        var resourceInfo = InstallModule();
        InstallModuleClasses(resourceInfo);
    }

    private ResourceInfo InstallModule()
    {
        var resourceInfo = resourceInfoProvider.Get(ResourceConstants.ResourceName)
            ?? new ResourceInfo();

        resourceInfo.ResourceDisplayName = ResourceConstants.ResourceDisplayName;
        resourceInfo.ResourceName = ResourceConstants.ResourceName;
        resourceInfo.ResourceDescription = ResourceConstants.ResourceDescription;
        resourceInfo.ResourceIsInDevelopment = ResourceConstants.ResourceIsInDevelopment;

        if (resourceInfo.HasChanged)
        {
            resourceInfoProvider.Set(resourceInfo);
        }

        return resourceInfo;
    }

    private static void InstallModuleClasses(ResourceInfo resourceInfo) => InstallRedirectClass(resourceInfo);

    private static void InstallRedirectClass(ResourceInfo resourceInfo)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(RedirectInfo.TYPEINFO.ObjectClassName) ??
                                      DataClassInfo.New(RedirectInfo.OBJECT_TYPE);

        info.ClassName = RedirectInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = RedirectInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Redirects";
        info.ClassResourceID = resourceInfo.ResourceID;
        info.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(RedirectInfo.RedirectID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(RedirectInfo.RedirectSourceUrl),
            Visible = true,
            DataType = FieldDataType.LongText,
            Enabled = true,
            AllowEmpty = false,
            Settings = new()
            {
                { nameof(TextInputProperties.Label), "Source URL" },
            }
        };
        formItem.SetComponentName(TextAreaComponent.IDENTIFIER);
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(RedirectInfo.RedirectTargetWebPageItemGUID),
            Visible = true,
            DataType = FieldDataType.Guid,
            Enabled = true,
            AllowEmpty = false,
            Settings = new()
            {
                { nameof(WebPageSelectorComponent.Properties.Label), "Target web page" },
                { nameof(WebPageSelectorComponent.Properties.ExplanationText), "Select the target web page to which requests for the source URL will be redirected to." },
                { nameof(WebPageSelectorComponent.Properties.ExplanationTextAsHtml), true },
                { nameof(WebPageSelectorComponent.Properties.MaximumPages), 1 },
                { nameof(WebPageSelectorComponent.Properties.ItemModifierType), typeof(WebPagesWithUrlWebPagePanelItemModifier) }
            }
        };
        formItem.SetComponentName(WebPageSelectorComponent.IDENTIFIER);
        formInfo.AddFormItem(formItem);
        
        formItem = new FormFieldInfo
        {
            Name = nameof(RedirectInfo.RedirectGUID),
            Visible = false,
            DataType = FieldDataType.Guid,
            Enabled = true,
            AllowEmpty = false,
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }

        PopulateRedirectGUIDs();
    }

    /// <summary>
    /// Ensure that the form is not upserted with any existing form
    /// </summary>
    /// <param name="info"></param>
    /// <param name="form"></param>
    private static void SetFormDefinition(DataClassInfo info, FormInfo form)
    {
        if (info.ClassID > 0)
        {
            var existingForm = new FormInfo(info.ClassFormDefinition);
            existingForm.CombineWithForm(form, new());
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }
    
    /// <summary>
    /// Retrospectively populates the RedirectGUID field for records created using version < v1.0.3 of this package
    /// </summary>
    private static void PopulateRedirectGUIDs()
    {
        List<RedirectInfo> redirectsWithEmptyGuids = RedirectInfo.Provider.Get().WhereEquals(nameof(RedirectInfo.RedirectGUID), Guid.Empty).ToList();

        if (redirectsWithEmptyGuids.Any())
        {
            foreach (RedirectInfo redirect in redirectsWithEmptyGuids)
            {
                redirect.RedirectGUID = new Guid();
                redirect.Update();
            }
        }
    }
}