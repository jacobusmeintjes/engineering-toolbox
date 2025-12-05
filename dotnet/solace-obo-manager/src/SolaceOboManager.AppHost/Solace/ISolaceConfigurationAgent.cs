using Refit;

namespace SolaceOboManager.AdminService.SolaceConfig;


public interface ISolaceConfigurationAgent
{
    [Post("/SEMP/v2/config/msgVpns/{msgVpnName}/clientUsernames")]
    Task CreateUser(string msgVpnName, [Body] MsgVpnClientUsername user);

    [Post("/SEMP/v2/config/msgVpns/{msgVpnName}/clientProfiles")]
    Task CreateClientProfile(string msgVpnName, [Body] MsgVpnClientProfile clientProfile);

    [Post("/SEMP/v2/config/msgVpns/{msgVpnName}/aclProfiles")]
    Task CreateAclProfile(string msgVpnName, [Body] MsgVpnAclProfile aclProfile);

    [Post("/SEMP/v2/config/msgVpns/{msgVpnName}/aclProfiles/{aclProfileName}/publishTopicExceptions")]
    Task CreatePublishTopicExceptions(string msgVpnName, string aclProfileName, [Body] MsgVpnAclProfilePublishTopicException aclProfileException);

}

