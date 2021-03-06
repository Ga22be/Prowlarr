using Nancy;
using Nancy.ErrorHandling;
using Prowlarr.Http.Extensions;

namespace Prowlarr.Http.ErrorManagement
{
    public class ErrorHandler : IStatusCodeHandler
    {
        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return true;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            if (statusCode == HttpStatusCode.SeeOther || statusCode == HttpStatusCode.MovedPermanently || statusCode == HttpStatusCode.OK)
            {
                return;
            }

            if (statusCode == HttpStatusCode.Continue)
            {
                context.Response = new Response { StatusCode = statusCode };
                return;
            }

            if (statusCode == HttpStatusCode.Unauthorized)
            {
                return;
            }

            if (context.Response.ContentType == "text/html" || context.Response.ContentType == "text/plain")
            {
                context.Response = new ErrorModel
                {
                    Message = statusCode.ToString()
                }.AsResponse(context, statusCode);
            }
        }
    }
}