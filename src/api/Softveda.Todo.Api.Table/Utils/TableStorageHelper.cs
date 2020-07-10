using Microsoft.Azure.Cosmos.Table;

namespace Softveda.Todo.Api.Table.Utils
{
	public static class TableStorageHelper
  {
    /// <summary>
    /// Get a reference to a Azure storage table
    /// </summary>
    /// <param name="storageConnectionString"></param>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public static CloudTable GetTableReference(string storageConnectionString, string tableName)
    {
      CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

      // Create a table client for interacting with the table service
      CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

      // Create a table client for interacting with the table service 
      CloudTable table = tableClient.GetTableReference(tableName);

      return table;

    }
  }
}
