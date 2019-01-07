using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;
using YamlDotNet.Serialization;

namespace RolemapperService.WebApi.Repositories
{
    public class KubernetesRepository : IKubernetesRepository
    {
        private readonly IKubernetes _client;
        private static readonly string ConfigMapName = "aws-auth";
        private static readonly string ConfigMapNamespace = "kube-system";

        public KubernetesRepository(IKubernetes client)
        {
            _client = client;
        }

        public async Task<string> GetAwsAuthConfigMap()
        {
            var awsAuthConfigMap = await _client.ReadNamespacedConfigMapAsync(name: ConfigMapName, namespaceParameter: ConfigMapNamespace);
            var awsAuthConfigMapYaml = SerializeToYaml(awsAuthConfigMap);

            return awsAuthConfigMapYaml;
        }

        public async Task<string> GetAwsAuthConfigMapRoleMap()
        {
            var awsAuthConfigMap = await _client.ReadNamespacedConfigMapAsync(name: ConfigMapName, namespaceParameter: ConfigMapNamespace);
            var awsAuthConfigMapMapRoles = awsAuthConfigMap.Data["mapRoles"];

            return awsAuthConfigMapMapRoles;
        }

        public async Task<string> ReplaceAwsAuthConfigMapRoleMap(string configMapRoleMap)
        {
            var configMap = await _client.ReadNamespacedConfigMapAsync(name: ConfigMapName, namespaceParameter: ConfigMapNamespace);
            configMap.Data = new Dictionary<string, string>
            {
                { "mapRoles", configMapRoleMap }
            };

            var awsAuthConfigMap = await _client.ReplaceNamespacedConfigMapAsync(body: configMap, name: ConfigMapName, namespaceParameter: ConfigMapNamespace);
            var awsAuthConfigMapYaml = SerializeToYaml(awsAuthConfigMap);

            return awsAuthConfigMapYaml;
        }

        public async Task<string> PatchAwsAuthConfigMapRoleMap(string configMapRoleMap)
        {
            var patch = new JsonPatchDocument<V1ConfigMap>();
            patch.Replace(c => c.Data["mapRoles"], configMapRoleMap);

            var configMapPatch = new V1Patch(patch);

            var awsAuthConfigMap = await _client.PatchNamespacedConfigMapAsync(body: configMapPatch, name: ConfigMapName, namespaceParameter: ConfigMapNamespace);
            var awsAuthConfigMapYaml = SerializeToYaml(awsAuthConfigMap);

            return awsAuthConfigMapYaml;
        }

        private string SerializeToYaml(object objectToSerialize)
        {
            var serializer = new SerializerBuilder().Build();
            var yamlSerializedObject = serializer.Serialize(objectToSerialize);

            return yamlSerializedObject;
        }
    }
}