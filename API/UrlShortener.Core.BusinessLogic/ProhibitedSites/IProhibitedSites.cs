namespace UrlShortener.Core.BusinessLogic
{
    public interface IProhibitedSites
    {
        bool VerifyIfLongUrlIsProhibited(string longUrl);
    }
}
