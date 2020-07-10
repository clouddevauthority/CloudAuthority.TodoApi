using Microsoft.Azure.Cosmos.Table;

namespace Softveda.Todo.Api.Table.Entities
{
  /// <summary>
  /// The azure table storage entity class for a Todo item
  /// </summary>
	public class TodoItemEntity : TableEntity
  {   
    public string Name { get; set; }
    public bool IsCompleted { get; set; }
  }
}
