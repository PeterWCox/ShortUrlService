using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UrlShortener.Core.Entities;
using UrlShortener.DataAccess;

namespace UrlShortener.Core.BusinessLogic
{
    public class UrlManager : IUrlManager
    {
        private readonly IDataConnector _dataConnection;
        private readonly IProhibitedSites _prohibitedSites;

        public UrlManager(IDataConnector dataConnection, IProhibitedSites prohibitedSites)
        {
            _dataConnection = dataConnection;
            _prohibitedSites = prohibitedSites;
        }

        //Properties
        private readonly Random Rng = new Random();

        /// <summary>
        /// A string that specifies the allowable characters that the ShortUrl path will be generated from.
        /// For example: Base36 = [a-z][0-9] => "abcdefghijklmnopqrstuvwxyz0123456789" 
        /// </summary>
        /// 
        public string EncodingString => Environment.GetEnvironmentVariable("EncodingString");   //Taken from JSON file if local, otherwise Azure Secret.

        /// <summary>
        /// An integer that specifies that number of characters the randomly generated short URL path will 
        /// contain. 
        /// For example: 6 => http://mottmac.com/d7929d contains only 6No. characters
        /// </summary>
        public int EncodingLength => int.Parse(Environment.GetEnvironmentVariable("EncodingLength"));


        /// <summary>
        /// The maximum number of attempts to generate a unique URL without clashing with the 
        /// existing Urls e.g. 10 => The CreateShortUrl method will attempt 10 times to create a 
        /// short URL and will give up after 10 collisions.
        /// </summary>
        public int MaximumNumberOfAttemptsGeneratingUniqueShortUrl => int.Parse(Environment.GetEnvironmentVariable("MaximumNumberOfAttemptsGeneratingUniqueShortUrl"));

        /// <summary>
        /// The maximum length of a specified longUrl to prevent abuse of the APIs - 
        /// refer to https://stackoverflow.com/questions/417142/what-is-the-maximum-length-of-a-url-in-different-browsers
        /// </summary>
        public int MaximumLengthOfLongUrl => int.Parse(Environment.GetEnvironmentVariable("MaximumLengthOfUrl"));


        //Methods
        public async Task<UrlModel> CreateUrlModelAsync(string longUrl, string domainName)
        {
            //Make sure that the entire long URL is lower case. 
            longUrl = longUrl.ToLower();

            //Ensure that the length of the URL is < maximum number of characters (typically 2048).
            if (longUrl.Length > MaximumLengthOfLongUrl)
            {
                throw new UrlTooLongException();
            }

            //Verify that the long URL is a syntactically valid URL. 
            if (VerifyLongUrlIsSyntacticallyCorrect(longUrl) == false)
            {
                throw new UrlNotSyntacticallyCorrectException();
            }

            //Verify that an existing short URL has not already been generated for the given longUrl.
            UrlModel urlModelExists = await _dataConnection.GetUrlModelByLongUrlAsync(longUrl);
            if (urlModelExists != null)
            {
                throw new UrlAlreadyExistsException();
            }

            //Verify that the site the URL is being generated for is not prohibited
            bool longUrlProhibited = _prohibitedSites.VerifyIfLongUrlIsProhibited(longUrl);
            if (longUrlProhibited)
            {
                throw new UrlProhibitedException();
            }

            //Try generating a unique short URL e.g. http://wwww.mottmac.com/3hd838 that has not already been used for a limited number of attempts as defined above 
            //and throw an exception if a unique short URL could not be generated after a given number of attempts.
            //Otherwise add a new UrlModel to the database. 
            int remainingAttempts = MaximumNumberOfAttemptsGeneratingUniqueShortUrl;
            bool isAUniqueShortUrl = false;
            string shortUrl = "";
            string segment = "";

            while (isAUniqueShortUrl == false && remainingAttempts > 0)
            {
                segment = GenerateRandomSegment();    //e.g. generate random string => 3jhd9s 
                UrlModel urlModel = await _dataConnection.GetUrlModelByShortUrlAsync(domainName, segment);
                isAUniqueShortUrl = (urlModel == null);
                remainingAttempts--;
            }

            if (isAUniqueShortUrl == false || remainingAttempts <= 0)
            {
                throw new UrlCouldNotBeGeneratedException();
            }
            else
            {
                var urlModel = new UrlModel()
                {
                    LongUrl = longUrl,
                    DomainName = domainName,
                    Segment = segment,
                    Added = DateTime.Now,
                    NumberOfClicks = 0
                };

                await _dataConnection.CreateUrlModelAsync(urlModel);

                return urlModel;
            }
        }

        public async Task<UrlModel> GetUrlModelByIdAsync(int id)
        {
            UrlModel url = await _dataConnection.GetUrlModelByIdAsync(id);

            switch (url)
            {
                case null:
                    throw new UrlNotFoundException();
                default:
                    return url;
            }
        }
            
        public async Task<UrlModel> GetUrlModelByShortUrlAsync(string domain, string identifier)
        {
            //Make sure that the entire short URL is lower case. 
            domain = domain.ToLower();
            identifier = identifier.ToLower();

            UrlModel urlModel = await _dataConnection.GetUrlModelByShortUrlAsync(domain, identifier);
            return urlModel;
        }

        public async Task<UrlModel> GetUrlModelByLongUrlAsync(string longUrl)
        {
            //Make sure that the entire long URL is lower case. 
            longUrl = longUrl.ToLower();

            UrlModel urlModel = await _dataConnection.GetUrlModelByLongUrlAsync(longUrl);
            return urlModel;
        }

        public async Task<List<UrlModel>> GetUrlModelAllAsync()
        {
            var urlModels = (await _dataConnection.GetUrlModelAllAsync()).ToList();
            return urlModels; 
        }

        public async Task UpdateUrlModelAsync(int id, string longUrl)
        {
            //Make the entire long URL lower case. 
            longUrl = longUrl.ToLower();

            //Verify that a URL Model exists corresponding to the specified id
            UrlModel url = await GetUrlModelByIdAsync(id);   
            if (url == null)
            {
                throw new UrlNotFoundException();
            }            
            
            //Verify that the length of the long URL is < maximum number of characters (typ. 2048). 
            if (longUrl.Length >= MaximumLengthOfLongUrl)
            {
                throw new UrlTooLongException();
            }

            //Verify that the long URL specified is syntactically correct i.e. a valid URL.
            if (VerifyLongUrlIsSyntacticallyCorrect(longUrl) == false)
            {
                throw new UrlNotSyntacticallyCorrectException();
            }

            //Verify that a URL model does not already exist for the specified long URL.
            bool shortUrlAlreadyExists = await _dataConnection.GetUrlModelByLongUrlAsync(longUrl) != null;
            if (shortUrlAlreadyExists)
            {
                throw new UrlAlreadyExistsException();
            }

            //Verify that the long URL is not a prohibited site. 
            bool longUrlProhibited = _prohibitedSites.VerifyIfLongUrlIsProhibited(longUrl);
            if (longUrlProhibited)
            {
                throw new UrlProhibitedException();
            }

            await _dataConnection.UpdateUrlModelByIdAsync(id, longUrl);
        }

        public async Task DeleteUrlModelAsync(int id)
        {
            UrlModel urlModel = await GetUrlModelByIdAsync(id);

            if (urlModel != null)
            {
                await _dataConnection.DeleteUrlModelByIdAsync(id);
            }
            else
            {
                throw new UrlNotFoundException();
            }
        }

        public async Task ClickShortUrlAsync(string domainName, string identifier)
        {
            await _dataConnection.ClickShortUrlAsync(domainName, identifier);
        }

        //URL Validators
        /// <summary>
        /// Returns boolean indicating whether a given URL is syntactically correct using ReGex.
        /// The URL must be prefixed with http or https (but the www portion is optional). 
        /// The string must not be null or empty (string.IsNullOrEmpty).
        /// </summary>
        /// <param name="url">The URL string to verify e.g. http://google.com </param>
        /// <returns></returns>
        public bool VerifyLongUrlIsSyntacticallyCorrect (string longUrl)
        {
            bool longUrlStartsWithHttpSOrHttp = (longUrl.StartsWith("https://") || longUrl.StartsWith("http://"));

            if (string.IsNullOrEmpty(longUrl))
            {
                return false;
            }
            else if (longUrlStartsWithHttpSOrHttp == false)
            {
                return false;
            }
            else
            {
                return Uri.IsWellFormedUriString(longUrl, UriKind.Absolute);
            }
        }

        public string GenerateRandomSegment()
        {
            string segment = "";

            for (int i = 0; i < EncodingLength; i++)
            {
                segment += EncodingString[Rng.Next(0, EncodingString.Length)];
            }

            return segment;
        }
    }
}
