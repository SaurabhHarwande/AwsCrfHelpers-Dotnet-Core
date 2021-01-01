using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CustomResourceHelper.Models;
using CustomResourceHelper.Models.Interfaces;

namespace CustomResourceHelper
{
    public class CrfRequestProcessor<TRequestProperties, TResponseData> where TRequestProperties : ICrfRequestProperties
    {
        public delegate TResponseData CrfRequestHandler(TRequestProperties input, TRequestProperties oldInput);
        public event CrfRequestHandler Create;
        public event CrfRequestHandler Update;
        public event CrfRequestHandler Delete;
        public async Task ProcessAsync(CrfRequest<TRequestProperties> crfRequestBody)
        {
            var RequestTypeEvent = (crfRequestBody.RequestType switch
            {
                "CREATE" => Create,
                "UPDATE" => Update,
                "DELETE" => Delete,
                _ => null
            });
            if(RequestTypeEvent == null)
                throw new Exception();
            var crfResponseData = RequestTypeEvent.Invoke(crfRequestBody.ResourceProperties, crfRequestBody.OldResourceProperties);
            
            var crfResponseBody = new CrfResponse<TResponseData>
            {
                Status = "SUCCESS",
                Reason = string.Empty,
                PhysicalResourceId = Guid.NewGuid().ToString(),
                StackId = crfRequestBody.StackId,
                RequestId = crfRequestBody.RequestId,
                LogicalResourceId = crfRequestBody.LogicalResourceId,
                Data = crfResponseData
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