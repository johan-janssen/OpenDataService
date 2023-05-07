using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using OpenDataService.DataSources;
using OpenDataService.Web.OData;
using OpenDataService.Web.Extensions;

namespace OpenDataService.Web.Extensions
{
    internal class MyODataRoutingMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        private IODataTemplateTranslator _translator;
        private IDataSourceProvider _provider;

        private Dictionary<IDataSource, IEdmModel> datasourceToModelMap = new Dictionary<IDataSource, IEdmModel>();

        public MyODataRoutingMatcherPolicy(IODataTemplateTranslator translator,
            IDataSourceProvider provider )
        {
            _translator = translator;
            _provider = provider;
        }

        public override int Order => 900 - 1;

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            return endpoints.Any(e => e.Metadata.OfType<ODataRoutingMetadata>().FirstOrDefault() != null);
        }

        public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            IODataFeature odataFeature = httpContext.ODataFeature();
            if (odataFeature.Path != null)
            {
                // If we have the OData path setting, it means there's some Policy working.
                // Let's skip this default OData matcher policy.
                return Task.CompletedTask;
            }

            for (var i = 0; i < candidates.Count; i++)
            {
                ref CandidateState candidate = ref candidates[i];
                if (!candidates.IsValidCandidate(i))
                {
                    continue;
                }

                IODataRoutingMetadata? metadata = candidate.Endpoint.Metadata.OfType<IODataRoutingMetadata>().FirstOrDefault();
                if (metadata == null)
                {
                    continue;
                }

                var dataSource = GetDataSourceFromRoute(candidate.Values);
                if (dataSource == null)
                {
                    continue;
                }
                httpContext.SetDataSource(dataSource);

                if (!datasourceToModelMap.TryGetValue(dataSource, out var model))
                {
                    model = new DataSourceModelBuilder().Build(dataSource);
                }

                ODataTemplateTranslateContext translatorContext = new ODataTemplateTranslateContext(httpContext, candidate.Endpoint, candidate.Values, model);

                ODataPath odataPath = _translator.Translate(metadata.Template, translatorContext);
                if (odataPath != null)
                {
                    odataFeature.RoutePrefix = metadata.Prefix;
                    odataFeature.Model = model;
                    odataFeature.Path = odataPath;

                    MergeRouteValues(translatorContext.UpdatedValues, candidate.Values);
                }
                else
                {
                    candidates.SetValidity(i, false);
                }
            }

            return Task.CompletedTask;
        }

        private static void MergeRouteValues(RouteValueDictionary updates, RouteValueDictionary? source)
        {
            if (source == null)
            {
                return;
            }

            foreach (var data in updates)
            {
                source[data.Key] = data.Value;
            }
        }

        private IDataSource? GetDataSourceFromRoute(RouteValueDictionary? routeValues)
        {
            if (routeValues == null)
            {
                return null;
            }

            if (!routeValues.TryGetValueTyped("datasource", out string? dataSourceName))
            {
                return null;
            }

            _provider.DataSources.TryGetValue(dataSourceName!, out IDataSource? dataSource);

            return dataSource;
        }
    }
}
