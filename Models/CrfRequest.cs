using CustomResourceHelper.Models.Interfaces;

namespace CustomResourceHelper.Models
{
    public class CrfRequest<T> where T : ICrfRequestProperties
    {
        public string RequestType { get; set; }
        public string ResponseURL { get; set; }
        public string StackId { get; set; }
        public string RequestId { get; set; }
        public string ResourceType { get; set; }
        public string LogicalResourceId{ get; set; }
        public string PhysicalResourceId { get; set; }
        public T ResourceProperties { get; set; }
        public T OldResourceProperties { get; set; }
    }
}
