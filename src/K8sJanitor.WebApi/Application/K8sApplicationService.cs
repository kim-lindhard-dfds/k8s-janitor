using System;
using System.Threading.Tasks;
using K8sJanitor.WebApi.Domain.Events;
using K8sJanitor.WebApi.Infrastructure.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace K8sJanitor.WebApi.Application
{
    public class K8sApplicationService : IK8sApplicationService
    {
        private readonly IServiceProvider _serviceProvider;

        public K8sApplicationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task FireEventK8sNamespaceCreatedAndAwsArnConnected(string namespaceName, Guid contextId)
        {
            var eventRegistry = _serviceProvider.GetRequiredService<DomainEventRegistry>();
            var eventsQueue = _serviceProvider.GetRequiredService<PublishingEventsQueue>();

            var evtPre = new K8sNamespaceCreatedAndAwsArnConnectedEvent(namespaceName, contextId);
            
            var evt = new DomainEventEnvelope
            {
                EventId = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Type = eventRegistry.GetTypeNameFor(evtPre),
                Format = "application/json",
                Data = JsonConvert.SerializeObject(evtPre, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                })
            };
            
            eventsQueue.AddEventToQueue(evt);
        }
        
        public Task TestCreated(string description)
        {
            throw new System.NotImplementedException();
        }
    }
}