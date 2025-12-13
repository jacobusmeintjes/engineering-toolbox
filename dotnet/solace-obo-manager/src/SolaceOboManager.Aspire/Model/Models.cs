namespace SolaceOboManager.Aspire.Model
{
    public record ClientProfile(string Name, bool ElidingEnabled = false, int ElidingDelay = 0);   
    
    public record AclProfile(string Name, string ClientConnectDefaultAction = "allow");

    public record PublishTopicException(string AclProfileName, string Name);

    public record ClientUser(string Username, string Password, bool SubscriptionManagerEnabled = false, string AclProfileName = "default", string ProfileName = "default");
}
