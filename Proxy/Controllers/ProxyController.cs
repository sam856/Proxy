using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Proxy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProxyController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> ProxyRequest([FromQuery] string url)
        {
            try
            {
                // Resolve the HttpClient from the factory
                var client = _httpClientFactory.CreateClient();

                // Create a new request message
                var requestMessage = new HttpRequestMessage
                {
                    Method = new HttpMethod(Request.Method),
                    RequestUri = new Uri(url),
                    Content = new StreamContent(Request.Body)
                };

                // Copy headers from the incoming request to the proxy request
                foreach (var (key, value) in Request.Headers)
                {
                    requestMessage.Headers.TryAddWithoutValidation(key, value.ToArray());
                }

                // Send the request and get the response
                var responseMessage = await client.SendAsync(requestMessage);

                // Copy response headers to the outgoing response
                foreach (var (key, value) in responseMessage.Headers)
                {
                    Response.Headers[key] = value.ToArray();
                }

                // Return the response content to the client
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                return Content(responseContent, responseMessage.Content.Headers.ContentType?.ToString());
            }
            catch (Exception ex)
            {
                return BadRequest($"Proxy request failed: {ex.Message}");
            }
        }
    }
}
