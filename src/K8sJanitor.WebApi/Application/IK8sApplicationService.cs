using System.Threading.Tasks;

namespace K8sJanitor.WebApi.Application
{
    public interface IK8sApplicationService
    {
        Task TestCreated(string description);
    }
}