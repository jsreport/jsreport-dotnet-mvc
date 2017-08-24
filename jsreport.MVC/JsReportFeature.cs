using jsreport.Types;
using System;
using System.Web;

namespace jsreport.MVC
{
    public interface IJsReportFeature
    {
        RenderRequest RenderRequest { get; set; }
        bool Enabled { get; set; }
        IJsReportFeature Configure(Action<RenderRequest> req);
        HttpContextBase Context { get; set; }        
        IJsReportFeature DebugLogsToResponse();
        IJsReportFeature NoBaseTag();
        IJsReportFeature Engine(Engine engine);
        IJsReportFeature Recipe(Recipe recipe);        
        Action<Report> AfterRender { get; set; }
        IJsReportFeature OnAfterRender(Action<Report> action);
    }

    public class JsReportFeature : IJsReportFeature
    {
        public JsReportFeature(HttpContextBase context)
        {
            RenderRequest = new RenderRequest();
            RenderRequest.Template.Engine = Types.Engine.None;            
            Context = context;
            RenderRequest.Options.Base = GetBaseUrl(context.Request);
            Enabled = true;
        }

        private string GetBaseUrl(HttpRequestBase request)
        {
            var appUrl = HttpRuntime.AppDomainAppVirtualPath;

            if (appUrl != "/")
                appUrl = "/" + appUrl;

            var baseUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);

            return baseUrl;
        }


        public RenderRequest RenderRequest { get; set; }
        public bool Enabled { get; set; }
        public HttpContextBase Context { get; set; }        
        public Action<Report> AfterRender { get; set; }

        public IJsReportFeature OnAfterRender(Action<Report> action)
        {
            AfterRender = action;
            return this;
        }

        public IJsReportFeature Engine(Engine engine)
        {
            RenderRequest.Template.Engine = engine;
            return this;
        }

        public IJsReportFeature Recipe(Recipe recipe)
        {
            RenderRequest.Template.Recipe = recipe;
            return this;
        }

        public IJsReportFeature NoBaseTag()
        {
            RenderRequest.Options.Base = null;
            return this;
        }        

        public IJsReportFeature DebugLogsToResponse()
        {
            RenderRequest.Options.Debug.LogsToResponse = true;
            return this;
        }

        public IJsReportFeature Configure(Action<RenderRequest> req)
        {
            req.Invoke(RenderRequest);
            return this;
        }
    }
}
