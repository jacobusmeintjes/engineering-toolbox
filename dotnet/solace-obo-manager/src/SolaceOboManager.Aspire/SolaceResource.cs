using Aspire.Hosting.ApplicationModel;
using Humanizer.Localisation;
using SolaceOboManager.Aspire.Model;
using SolaceOboManager.Aspire.Solace;

namespace SolaceOboManager.Aspire
{
    public class SolaceResource(string name, ParameterResource? adminPassword = null) : ContainerResource(name), IResourceWithEnvironment
    {
        public const string DefaultRegistry = "docker.io";
        public const string DefaultImage = "solace/solace-pubsub-standard";
        public const string DefaultTag = "latest";

        public string AdminUsername => "admin";
        public ParameterResource AdminPasswordParameter { get; } = adminPassword ?? new ParameterResource("solace-admin-password", x => "admin", true);

        private readonly List<ClientProfile> _clientProfiles = new List<ClientProfile>();
        private readonly List<AclProfile> _aclProfiles = new List<AclProfile>();
        private readonly List<PublishTopicException> _publishTopicExceptions = new List<PublishTopicException>();
        private readonly List<ClientUser> _users = new List<ClientUser>();

        internal List<MsgVpnClientProfile> ClientProfiles => _clientProfiles.Select(cp => new MsgVpnClientProfile
        {
            ClientProfileName = cp.Name,
            ElidingEnabled = cp.ElidingEnabled,
            ElidingDelay = cp.ElidingDelay,
        }).ToList();

        internal List<MsgVpnAclProfile> AclProfiles => _aclProfiles.Select(ap => new MsgVpnAclProfile
        {
            AclProfileName = ap.Name,
            ClientConnectDefaultAction = ap.ClientConnectDefaultAction,
        }).ToList();

        internal List<MsgVpnAclProfilePublishTopicException> PublishTopicExceptions => _publishTopicExceptions.Select(te => new MsgVpnAclProfilePublishTopicException
        {
            AclProfileName = te.AclProfileName,
            PublishTopicException = te.Name,
            VpnName = "default",
        }).ToList();

        internal List<MsgVpnClientUsername> Users => _users.Select(u => new MsgVpnClientUsername
        {
            Username = u.Username,
            Password = u.Password,
            SubscriptionManagerEnabled = u.SubscriptionManagerEnabled,
            ProfileName = u.ProfileName,
            AclProfileName = u.AclProfileName
        }).ToList();

        public void AddClientProfile(ClientProfile clientProfile)
        {
            _clientProfiles.Add(clientProfile);
        }

        public void AddAclProfile(AclProfile aclProfile)
        {
            _aclProfiles.Add(aclProfile);
        }

        public void AddPublishTopicException(PublishTopicException publishTopicException)
        {
            _publishTopicExceptions.Add(publishTopicException);
        }

        public void AddUser(ClientUser user)
        {
            _users.Add(user);
        }
    }
}
