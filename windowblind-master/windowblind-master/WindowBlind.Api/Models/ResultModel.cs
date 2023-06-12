using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace WindowBlind.Api.Models
{
    public class ResultModel
    {
        public HttpStatusCode Status { get; set; }
        public object Data { get; set; }
        public string Message { get; set; }
        public string StackTrace {  get; set; }

    }
}
