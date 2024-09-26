using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.Redirects.Admin;

[assembly: CMS.RegisterModule(typeof(RedirectModule))]
namespace XperienceCommunity.Redirects.Admin;

internal class RedirectModule : Module
{
    private IRedirectModuleInstaller? _installer;

    public RedirectModule() : base(nameof(RedirectModule))
    {
    }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        var services = parameters.Services;

        _installer = services.GetRequiredService<IRedirectModuleInstaller>();

        ApplicationEvents.Initialized.Execute += InitializeModule;
    }

    private void InitializeModule(object? sender, EventArgs e) => _installer?.Install();
}
