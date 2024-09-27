using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class APIResponse
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.BadRequest;

        public bool IsSuccess { get; set; } = false;

        public List<string>? Errors { get; set; } = new List<string>();

        public object? Result { get; set; } = null;
    }
}
