using System.Net;

namespace WindowBlind.Api.Models
{
    public class ResultModel
    {
        public HttpStatusCode Status { get; set; }
        public object Data { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }

    }
}
