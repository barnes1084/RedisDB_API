namespace RedisApi
{
    public class TableService
    {
        private RedisConnectorHelper _redisHelper;

        public TableService(RedisConnectorHelper redisHelper)
        {
            _redisHelper = redisHelper;
        }

        public bool InsertTableRow(string tableName, int id, Dictionary<string, object> row)
        {
            string key = $"{tableName}:{id}";
            return _redisHelper.InsertRecord(key, row);
        }

        public Dictionary<string, string> GetEntryOrField(string tableName, int id, string fieldName)
        {
            string key = $"{tableName}:{id}";
            if (string.IsNullOrEmpty(fieldName))
            {
                return _redisHelper.GetEntry(key);
            }
            else
            {
                var fieldValue = _redisHelper.GetFieldValue(key, fieldName);
                if (fieldValue != null)
                {
                    return new Dictionary<string, string> { { fieldName, fieldValue } };
                }
                return null;
            }
        }


        public async Task<List<string>> GetAllTableNames()
        {
            return await _redisHelper.GetAllTableNames();
        }


        public async Task<Dictionary<string, Dictionary<string, string>>> GetAllEntriesForTable(string tableName)
        {
            return await _redisHelper.GetAllEntriesForTable(tableName);
        }

        public bool DeleteEntry(string tableName, int id)
        {
            return _redisHelper.DeleteEntry(tableName, id);
        }

    }
}
