using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using K8sJanitor.WebApi.Application;
using K8sJanitor.WebApi.Domain.Events;
using K8sJanitor.WebApi.Infrastructure.AWS;
using K8sJanitor.WebApi.Models;
using K8sJanitor.WebApi.Repositories.Kubernetes;
using K8sJanitor.WebApi.Services;
using Microsoft.Extensions.DependencyInjection;

namespace K8sJanitor.WebApi.EventHandlers
{
    public class ContextAccountCreatedDomainEventHandler  : IEventHandler<ContextAccountCreatedDomainEvent>
    {
        private readonly IConfigMapService _configMapService;
        private readonly INamespaceRepository _namespaceRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IRoleBindingRepository _roleBindingRepository;
        private readonly IK8sApplicationService _k8sApplicationService;

        public ContextAccountCreatedDomainEventHandler(
            IConfigMapService configMapService,
            INamespaceRepository namespaceRepository,
            IRoleRepository roleRepository,
            IRoleBindingRepository roleBindingRepository,
            IK8sApplicationService k8sApplicationService
        )
        {
            _configMapService = configMapService;
            _namespaceRepository = namespaceRepository;
            _roleRepository = roleRepository;
            _roleBindingRepository = roleBindingRepository;
            _k8sApplicationService = k8sApplicationService;
        }


        public async Task HandleAsync(ContextAccountCreatedDomainEvent domainEvent)
        {
            var namespaceName = NamespaceName.Create(domainEvent.Payload.CapabilityRootId);
            
            await CreateNameSpace(namespaceName, domainEvent);

            await ConnectAwsArnToNameSpace(namespaceName, domainEvent.Payload.RoleArn);

            var namespaceRoleName = await _roleRepository
                .CreateNamespaceFullAccessRole(namespaceName);

            await _roleBindingRepository.BindNamespaceRoleToGroup(
                namespaceName: namespaceName,
                role: namespaceRoleName,
                group: namespaceName
            );

            await _k8sApplicationService.FireEventK8sNamespaceCreatedAndAwsArnConnected(namespaceName, domainEvent.Payload.ContextId, domainEvent.Payload.CapabilityId); // Emit Kafka event "k8s_namespace_created_and_aws_arn_connected"
        }

        public async Task CreateNameSpace(
            NamespaceName namespaceName,
            ContextAccountCreatedDomainEvent domainEvent
        )
        {
            var labels = new List<Label>
            {
                Label.CreateSafely("capability-id", domainEvent.Payload.CapabilityId.ToString()),
                Label.CreateSafely("capability-name", domainEvent.Payload.CapabilityName),
                Label.CreateSafely("context-id", domainEvent.Payload.ContextId.ToString()),
                Label.CreateSafely("context-name", domainEvent.Payload.ContextName)
            };

            await _namespaceRepository.CreateNamespaceAsync(namespaceName, labels);
            await _namespaceRepository.AddAnnotations(namespaceName, new Dictionary<string, string>
            {
                {
                    "iam.amazonaws.com/permitted",
                    IAM.ConstructRoleArn(domainEvent.Payload.AccountId, "*")
                }
            });
        }

        public async Task ConnectAwsArnToNameSpace(NamespaceName namespaceName, string roleArn)
        {
            var roleName = namespaceName;

            await _configMapService.AddRole(
                roleName: roleName,
                roleArn: roleArn
            );
            var annotations = new Dictionary<string, string>();
            await _namespaceRepository.AddAnnotations(namespaceName, annotations);
        } 
    }
}