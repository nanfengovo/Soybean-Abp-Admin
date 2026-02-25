using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace RCS.LogManage
{
    public abstract class ApiLogBase : CreationAuditedEntity<long>
    {

        /// <summary>
        /// Gets or sets the IPv4 or IPv6 address associated with the entity.
        /// </summary>
        public string IPAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URL associated with this instance.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the HTTP method to be used for the request (for example, "GET", "POST", or "PUT").
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the API associated with this instance.
        /// </summary>
        public string ApiName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of the request header to be sent with an HTTP request.
        /// </summary>
        public string RequestHeader { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the raw body content of the HTTP request.
        /// </summary>
        public string RequestBody { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the body content of the HTTP response as a string.
        /// </summary>
        public string ResponseBody { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the HTTP status code associated with the response.
        /// </summary>
        public int StatusCode { get; set; }

        public long Duration { get; set; }
    }
}
