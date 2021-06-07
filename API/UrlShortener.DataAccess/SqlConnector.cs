using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using UrlShortener.Core.Entities;

namespace UrlShortener.DataAccess
{
    public class SqlConnector : IDataConnector
    {
        private static string ConnectionString => Environment.GetEnvironmentVariable("sqldb_connection");
        private static string TableName => "dbo.UrlModel";

        //Create/Update/Delete/"Click" Methods
        private async Task WriteUrlModelAsync(string connectionString, string sql, object p)
        {
            using (IDbConnection cnn = new SqlConnection(connectionString))
            {
                await cnn.ExecuteAsync(sql, p); //Hello
            }
        }

        public async Task CreateUrlModelAsync(UrlModel urlModel)
        {
            string sql =
                    $"INSERT INTO {TableName} (LongUrl, DomainName, Segment, Added, NumberOfClicks) " +
                    "VALUES (@LongUrl, @DomainName, @Segment, @Added, @NumberOfClicks) SELECT CAST(SCOPE_IDENTITY() as int);";

            var p = new
            {
                LongUrl = urlModel.LongUrl,
                DomainName = urlModel.DomainName,
                Segment = urlModel.Segment,
                Added = urlModel.Added,
                NumberOfClicks = urlModel.NumberOfClicks
            };

            await WriteUrlModelAsync(ConnectionString, sql, p);

            //We need to set the id of the urlNModel by reference here. 
            var newUrlModel = await GetUrlModelByLongUrlAsync(urlModel.LongUrl);
            urlModel.Id = newUrlModel.Id;
        }

        public async Task UpdateUrlModelByIdAsync(int id, string longUrl)
        {
            string sql = $"UPDATE {TableName} SET LongUrl = @LongUrl WHERE id = @id;";
            var p = new
            {
                Id = id,
                LongUrl = longUrl,
            };

            await WriteUrlModelAsync(ConnectionString, sql, p);
        }

        public async Task DeleteUrlModelByIdAsync(int id)
        {
            string sql = $"DELETE FROM {TableName} WHERE Id = @id;";
            var p = new {Id = id};
            await WriteUrlModelAsync(ConnectionString, sql, p);

        }

        public async Task ClickShortUrlAsync(string domainName, string segment)
        {
            string sql = $"UPDATE {TableName} SET NumberOfClicks = NumberOfClicks + 1 WHERE DomainName = @DomainName AND Segment = @Segment";
            var p = new
            {
                DomainName = domainName,
                Segment = segment,
            };

            await WriteUrlModelAsync(ConnectionString, sql, p);
        }

        //Read Methods 
        private async Task<IEnumerable<UrlModel>> ReadUrlModel_All(string connectionString, string sql, object p)
        {
            using (IDbConnection cnn = new SqlConnection(connectionString))
            {
                Task<IEnumerable<UrlModel>> output = cnn.QueryAsync<UrlModel>(sql, p);
                return await output;
            }
        }

        private async Task<UrlModel> ReadUrlModel_SingleOrDefault(string connectionString, string sql, object p)
        {
            using (IDbConnection cnn = new SqlConnection(connectionString))
            {
                Task<UrlModel> output = cnn.QuerySingleOrDefaultAsync<UrlModel>(sql, p);
                return await output;
            }
        }

        public async Task<IEnumerable<UrlModel>> GetUrlModelAllAsync()
        {
            string sql = $"SELECT * FROM {TableName}";
            Task<IEnumerable<UrlModel>> output = ReadUrlModel_All(ConnectionString, sql, null);
            return await output;
        }

        public async Task<UrlModel> GetUrlModelByIdAsync(int id)
        {
            string sql = $"SELECT * FROM {TableName} WHERE Id = @Id;";
            Task<UrlModel> output = ReadUrlModel_SingleOrDefault(ConnectionString, sql, new { Id = id });
            return await output;
        }

        public async Task<UrlModel> GetUrlModelByShortUrlAsync(string domainName, string segment)
        {
            string sql = $"SELECT * FROM {TableName} WHERE DomainName = @DomainName AND Segment = @Segment;";

            var p = new
            {
                DomainName = domainName,
                Segment = segment,
            };

            Task<UrlModel> output = ReadUrlModel_SingleOrDefault(ConnectionString, sql, p);
            return await output;
        }

        public async Task<UrlModel> GetUrlModelByLongUrlAsync(string longUrl)
        {
            string sql = $"SELECT * FROM {TableName} WHERE LongUrl = @LongUrl;";
            Task<UrlModel> output = ReadUrlModel_SingleOrDefault(ConnectionString, sql, new { LongUrl = longUrl });
            return await output;
        }
    }
}

