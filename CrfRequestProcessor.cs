using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CrfHelpers.Models;
using CrfHelpers.Models.Interfaces;

namespace CrfHelpers
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
            var RequestTypeEvent = crfRequestBody.RequestType switch
            {
                "Create" => Create,
                "Update" => Update,
                "Delete" => Delete,
                _ => null
            };
            if(RequestTypeEvent == null)
                throw new Exception();
            var crfResponseData = await RequestTypeEvent.Invoke(crfRequestBody.ResourceProperties, crfRequestBody.OldResourceProperties);
            
            var crfResponseBody = new CrfResponseBody<TResponseData>
            {
                Status = "SUCCESS",
                Reason = string.Empty,
                PhysicalResourceId = Guid.NewGuid().ToString(),
                StackId = crfRequestBody.StackId,
                RequestId = crfRequestBody.RequestId,
                LogicalResourceId = crfRequestBody.LogicalResourceId,
                Data = crfResponseData ?? new TResponseData {}
            };

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