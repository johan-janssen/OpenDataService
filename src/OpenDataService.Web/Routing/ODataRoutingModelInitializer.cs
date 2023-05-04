using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using OpenDataService.Web.Controllers;

namespace OpenDataService.Web.Routing;

public class ODataRoutingModelInitializer : IApplicationModelProvider
{
    public ODataRoutingModelInitializer(IOptions<ODataOptions> options)
    {
        options.Value.AddRouteComponents("odata/{datasource}", EdmCoreModel.Instance);
    }

    /// <summary>
    /// Gets the order value for determining the order of execution of providers.
    /// </summary>
    public int Order => 90;

    public void OnProvidersExecuted(ApplicationModelProviderContext context)
    {
        EdmModel model = new EdmModel();
        const string prefix = "odata/{datasource}";

        ProcessHandleAll(prefix, model, context.Result.Controllers.Single(controller => controller.ControllerType == typeof(HandleAllController)));
        ProcessMetadata(prefix, model, context.Result.Controllers.Single(controller => controller.ControllerType == typeof(MetadataController)));
    }

    public void OnProvidersExecuting(ApplicationModelProviderContext context)
    {
    }

    private void ProcessHandleAll(string prefix, IEdmModel model, ControllerModel controllerModel)
    {
        controllerModel.Actions.Single(action => action.ActionName == "GetNavigation")
            .AddSelector("get", prefix, model, new ODataPathTemplate(
                    new EntitySetTemplateSegment(),
                    new EntitySetWithKeyTemplateSegment(),
                    new NavigationTemplateSegment()));

        controllerModel.Actions.Single(action => action.ActionName == "Get")
            .AddSelector("get", prefix, model, new ODataPathTemplate(new EntitySetTemplateSegment()));
        
        controllerModel.Actions.Single(action => action.ActionName == "GetByKey")
            .AddSelector("get", prefix, model, new ODataPathTemplate(new EntitySetTemplateSegment(), new EntitySetWithKeyTemplateSegment()));
        
        // foreach (var actionModel in controllerModel.Actions)
        // {
        //     if (actionModel.ActionName == "GetName")
        //     {
        //         ODataPathTemplate path = new ODataPathTemplate(
        //             new EntitySetTemplateSegment(),
        //             new EntitySetWithKeyTemplateSegment(),
        //             new StaticNameSegment());

        //         actionModel.AddSelector("get", prefix, model, path);
        //     }
        // }
    }

    private void ProcessMetadata(string prefix, IEdmModel model, ControllerModel controllerModel)
    {
        controllerModel.Actions.Single(action => action.ActionName == "GetMetadata")
            .AddSelector("get", prefix, model, new ODataPathTemplate(MetadataSegmentTemplate.Instance));
        
        controllerModel.Actions.Single(action => action.ActionName == "GetServiceDocument")
            .AddSelector("get", prefix, model, new ODataPathTemplate());
    }
}