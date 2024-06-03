    using StackExchange.Redis;
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

namespace RedisApi
{
    public class RedisConnectorHelper
    {
        private static ConnectionMultiplexer redisConnection;
        private readonly string _redisConnectionString;

        public RedisConnectorHelper(string connectionString)
        {
            _redisConnectionString = connectionString;
        }

        public IDatabase GetDatabase()
        {
            try
            {
                if (redisConnection == null || !redisConnection.IsConnected)
                {
                    redisConnection = ConnectionMultiplexer.Connect(_redisConnectionString);
                }
                return redisConnection.GetDatabase();
            }
            catch (RedisConnectionException ex)
            {
                throw new ApplicationException("Unable to connect to Redis", ex);
            }
        }

        public IServer GetServer()
        {
            var endpoint = redisConnection.GetEndPoints().First();
            return redisConnection.GetServer(endpoint);
        }

        public bool InsertRecord(string key, Dictionary<string, object> tableRow)
        {
            try
            {
                var db = GetDatabase();
                var entries = tableRow.Select(kv => new HashEntry(kv.Key, kv.Value.ToString())).ToArray();
                db.HashSet(key, entries);
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error inserting record into Redis", ex);
            }
        }

        public string GetFieldValue(string key, string fieldName)
        {
            var db = GetDatabase();
            RedisValue value = db.HashGet(key, fieldName);
            return value.IsNull ? null : value.ToString();
        }

        public Dictionary<string, string> GetEntry(string key)
        {
            var db = GetDatabase();
            var entries = db.HashGetAll(key);
            return entries.ToStringDictionary();
        }

        public async Task<List<string>> GetAllTableNames()
        {
            var db = GetDatabase();
            var server = GetServer(); 
            var tableNames = new HashSet<string>();

            // Using the 'SCAN' command with pattern to get all keys
            await foreach (var key in server.KeysAsync(pattern: "*:*"))
            {
                var tableName = key.ToString().Split(':')[0]; 
                tableNames.Add(tableName);
            }

            return tableNames.ToList();
        }


        public async Task<Dictionary<string, Dictionary<string, string>>> GetAllEntriesForTable(string tableName)
        {
            var db = GetDatabase();
            var server = GetServer();
            var entries = new Dictionary<string, Dictionary<string, string>>();

            // Scan for all keys that belong to the table
            var keys = server.Keys(pattern: $"{tableName}:*").ToArray();
            foreach (var key in keys)
            {
                var hashEntries = db.HashGetAll(key);
                var data = hashEntries.ToStringDictionary();
                entries[key.ToString()] = data;
            }

            return entries;
        }

        public bool DeleteEntry(string tableName, int id)
        {
            var db = GetDatabase();
            var key = $"{tableName}:{id}";
            return db.KeyDelete(key); 
        }


    }
}
