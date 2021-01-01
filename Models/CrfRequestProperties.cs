using CustomResourceHelper.Models.Interfaces;

namespace CustomResourceHelper.Models
{
    public class CrfRequestProperties : ICrfRequestProperties
    {
        string ICrfRequestProperties.ServiceToken { get; set; }
        public string ListenerArn { get; set; }
    }
}
