using System;

namespace MinecraftWrapper.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty ( RequestId );

        public Exception Exception { get; set; }

        public bool ShowException => Exception != null;
    }
}