using System;

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using jsreport.Types;
using jsreport.Shared;

namespace jsreport.MVC
{
    public class JsReportFilterAttribute : Attribute, IActionFilter
    {
        public JsReportFilterAttribute(IRenderService renderService)
        {
            RenderService = renderService;
        }    

        protected IRenderService RenderService { get; set; }

        public virtual void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.HttpContext.Items["jsreportFeature"] = new JsReportFeature(filterContext.HttpContext);
        }

        public virtual void OnActionExecuted(ActionExecutedContext filterContext)
        {
            EnableJsReportAttribute attr;
            if (ShouldUseJsReport(filterContext, out attr))
            {                
                filterContext.HttpContext.Response.Filter = new JsReportStream(filterContext, attr, RenderReport);
            }
        }    

        protected virtual RenderRequest CreateRenderingRequest(ActionExecutedContext context,
                                                          EnableJsReportAttribute jsreportAttribute, string htmlContent)
        {
            return context.HttpContext.JsReportFeature().RenderRequest;
        }    

        protected virtual async Task<Report> RenderReport(ActionExecutedContext context,
                                                          EnableJsReportAttribute jsreportAttribute, string htmlContent)
        {
            IJsReportFeature feature = context.HttpContext.JsReportFeature();
            var renderingRequest = CreateRenderingRequest(context, jsreportAttribute, htmlContent);
            renderingRequest.Template.Content = htmlContent;
            Report output = await RenderService.RenderAsync(renderingRequest).ConfigureAwait(false);
           
            AddResponseHeaders(context, jsreportAttribute, output);
            feature.AfterRender?.Invoke(output);

            return output;
        }

        protected virtual void AddResponseHeaders(ActionExecutedContext context, EnableJsReportAttribute jsreportAttribute,
                                               Report report)
        {
            context.HttpContext.Response.ContentType = report.Meta.ContentType;
            context.HttpContext.Response.Headers["Content-Disposition"] = report.Meta.ContentDisposition;           
        }       

        protected virtual bool ShouldUseJsReport(ActionExecutedContext filterContext, out EnableJsReportAttribute attr)
        {
            if ((filterContext.Exception != null && !filterContext.ExceptionHandled) || filterContext.Canceled)
            {
                attr = null;
                return false;
            }

            if (!filterContext.Controller.ViewData.ModelState.IsValid)
            {
                attr = null;
                return false;
            }

            bool enableJsReport = false;
            attr = null;

            if (filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof (EnableJsReportAttribute), true))
            {
                attr =
                    (EnableJsReportAttribute)
                    filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(
                        typeof (EnableJsReportAttribute), true).First();
                enableJsReport = true;
            }

            if (filterContext.ActionDescriptor.IsDefined(typeof (EnableJsReportAttribute), true))
            {
                attr =
                    (EnableJsReportAttribute)
                    filterContext.ActionDescriptor.GetCustomAttributes(typeof (EnableJsReportAttribute), true).First();
                enableJsReport = true;
            }

            return enableJsReport;
        }
    }
}