namespace conduit_api.Features.Profiles
{
    public class ProfileEnvelope
    {
        public ProfileEnvelope(Profile profile)
        {
            Profile = profile;
        }

        public Profile Profile { get; }
    }
}