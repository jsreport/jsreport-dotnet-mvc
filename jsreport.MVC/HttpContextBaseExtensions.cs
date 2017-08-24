using jsreport.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace jsreport.MVC
{
    public static class HttpContextBaseExtensions
    {
        public static IJsReportFeature JsReportFeature(this HttpContextBase context)
        {
            return (IJsReportFeature) context.Items["jsreportFeature"];
        }

        public static Template JsReportTemplate(this HttpContextBase context)
        {
            return context.JsReportFeature().RenderRequest.Template;
        }

        public static RenderRequest JsReportRequest(this HttpContextBase context)
        {
            return context.JsReportFeature().RenderRequest;
        }
    }
}
