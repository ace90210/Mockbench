using System;

namespace Mockbench.Shared.Models.Configuration
{
    public class DatabaseConfig : ICopyTo<DatabaseConfig>
    {
        public DatabaseProvider Provider { get; set; } = DatabaseProvider.Sqlite;

        public string ConnectionString { get; set; } = "Data Source= Mockbench.db;";

        public int GetId()
        {
            return 0;
        }
        
        public DatabaseConfig CopyTo(DatabaseConfig target)
        { 
            if (target == null)
                throw new NotSupportedException($"{nameof(DatabaseConfig)}: Cannot copy to a null target");
            
            target.Provider = Provider;
            target.ConnectionString = ConnectionString;
             
            return target;
        }
    }
}
