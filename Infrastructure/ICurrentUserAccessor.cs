namespace conduit_api.Infrastructure
{
    public interface ICurrentUserAccessor
    {
        string GetCurrentUsername();
    }
}