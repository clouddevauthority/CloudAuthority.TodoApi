using Softveda.Todo.Shared.Models;

namespace Softveda.Todo.Api.Table.Entities
{
  /// <summary>
  /// Provides methods to map from a Todo item table storage entity to model and vice versa
  /// </summary>
	public static class TodoItemMapper
  {
    public static readonly string TablePartitionKey = "TodoItem";
    public static TodoItemEntity ToEntity(this TodoItem todoItemModel) =>     
      new TodoItemEntity()
      {
        PartitionKey = TablePartitionKey,
        RowKey = todoItemModel.Id,
        Name = todoItemModel.Name,
        IsCompleted = todoItemModel.IsCompleted
      };
 

    public static TodoItem ToModel(this TodoItemEntity todoItemEntity) =>
      new TodoItem()
      {
        Id = todoItemEntity.RowKey,
        Name = todoItemEntity.Name,
        IsCompleted = todoItemEntity.IsCompleted
      };

  }
}
