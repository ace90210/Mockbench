namespace Mockbench.Shared.Models.Configuration
{
    public class DatabaseConfig : ICopyTo<DatabaseConfig>
    {
        public DatabaseProvider Provider { get; set; } = DatabaseProvider.InMemory;

        public string MainConnectionString { get; set; } = "Data Source= Mockbench.Main.db;";

        public string AuthenticationConnectionString { get; set; } = "Data Source= Mockbench.Authentication.db;";

        public int GetId()
        {
            return 0;
        }
        
        public DatabaseConfig CopyTo(DatabaseConfig target)
        { 
            if (target == null)
                throw new NotSupportedException($"{nameof(DatabaseConfig)}: Cannot copy to a null target");
            
            target.Provider = Provider;
            target.MainConnectionString = MainConnectionString;
            target.AuthenticationConnectionString = AuthenticationConnectionString;

            return target;
        }
    }
}
