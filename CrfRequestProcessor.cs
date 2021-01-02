using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AwsCrfHelpers.Models;
using AwsCrfHelpers.Models.Interfaces;

namespace AwsCrfHelpers
{
    public class CrfRequestProcessor<TRequestProperties, TResponseData>
        where TRequestProperties : ICrfRequestProperties
        where TResponseData : new()
    {
        public delegate Task<TResponseData> CrfRequestHandler(TRequestProperties requestProperties, TRequestProperties oldRequestProperties);
        public event CrfRequestHandler Create;
        public event CrfRequestHandler Update;
        public event CrfRequestHandler Delete;
        public async Task ProcessAsync(CrfRequestBody<TRequestProperties> crfRequestBody)
        {
            CrfResponseBody<TResponseData> crfResponseBody;
            
            try
            {
                var RequestTypeEvent = crfRequestBody.RequestType switch
                {
                    "Create" => Create,
                    "Update" => Update,
                    "Delete" => Delete,
                    _ => null
                };
                if(RequestTypeEvent == null)
                    throw new Exception($"{crfRequestBody.RequestType} event is not defined.");
                var crfResponseData = await RequestTypeEvent.Invoke(crfRequestBody.ResourceProperties, crfRequestBody.OldResourceProperties);
                
                crfResponseBody = new CrfResponseBody<TResponseData>
                {
                    Status = "SUCCESS",
                    Reason = string.Empty,
                    PhysicalResourceId = Guid.NewGuid().ToString(),
                    StackId = crfRequestBody.StackId,
                    RequestId = crfRequestBody.RequestId,
                    LogicalResourceId = crfRequestBody.LogicalResourceId,
                    Data = crfResponseData ?? new TResponseData()
                };
            }
            catch(Exception e)
            {
                crfResponseBody = new CrfResponseBody<TResponseData>
                {
                    Status = "FAILED",
                    Reason = e.Message,
                    PhysicalResourceId = Guid.NewGuid().ToString(),
                    StackId = crfRequestBody.StackId,
                    RequestId = crfRequestBody.RequestId,
                    LogicalResourceId = crfRequestBody.LogicalResourceId,
                    Data = new TResponseData()
                };
            }

            var crfResponseJson = JsonSerializer.Serialize(crfResponseBody);
            var crfResponseBytes = Encoding.UTF8.GetBytes(crfResponseJson);

            var webRequest = WebRequest.Create(crfRequestBody.ResponseURL) as HttpWebRequest;
            webRequest.Method = "PUT";
            webRequest.ContentType = string.Empty;
            webRequest.ContentLength = crfResponseBytes.LongLength;
            using(var webRequestStream = await webRequest.GetRequestStreamAsync())
            {
                await webRequestStream.WriteAsync(crfResponseBytes, 0, crfResponseBytes.Length);
            }
            await webRequest.GetResponseAsync();
        }
    }
}