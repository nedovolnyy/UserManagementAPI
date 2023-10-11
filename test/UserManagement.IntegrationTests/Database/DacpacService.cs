namespace UserManagement.IntegrationTests.Database;

public class DacpacService
{
    public void ProcessDacPac(string connectionString,
                                string databaseName,
                                string dacpacName)
    {
        var dacOptions = new DacDeployOptions
        {
            BlockOnPossibleDataLoss = false,
        };

        var dacServiceInstance = new DacServices(connectionString);

        using var dacpac = DacPackage.Load(dacpacName);
        dacServiceInstance.Deploy(dacpac, databaseName,
                                upgradeExisting: true,
                                options: dacOptions);
    }
}
