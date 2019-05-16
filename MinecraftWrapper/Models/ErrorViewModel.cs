using System;

namespace MinecraftWrapper.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty ( RequestId );

        public Exception Exception { get; set; }
        public string ExceptionHtml => GetExceptionHtmlString ( Exception );

        public bool ShowException => Exception != null;

        private string GetExceptionHtmlString ( Exception ex )
        {
            var htmlOut = "";

            if ( ex != null )
            {
                htmlOut += "<p>";
                htmlOut += $"<span>{ex.Message}:</span>";
                htmlOut += $"<span>{ex.StackTrace.Replace(Environment.NewLine,"<br/>")}</span>";
                htmlOut += "</p>";
            }

            if ( ex.InnerException != null )
            {
                htmlOut += "<h4>Inner Exception:</h4>";
                htmlOut += GetExceptionHtmlString ( ex.InnerException );
            }

            return  htmlOut;
        }

    }
}