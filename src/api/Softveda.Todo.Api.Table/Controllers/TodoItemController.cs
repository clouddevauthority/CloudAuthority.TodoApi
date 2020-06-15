using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Softveda.Todo.Api.Table.Entities;
using Softveda.Todo.Api.Table.Utils;
using Softveda.Todo.Shared.Models;

namespace Softveda.Todo.Api.Table.Controllers
{
	[Route("table/[controller]")]
	[Produces("application/json")]
	[ApiController]
	public class TodoItemController : ControllerBase
	{
		private readonly ILogger<TodoItemController> _logger;
		private readonly IConfiguration _configuration;
		private readonly string _storageConnectionString;
		private readonly string _tableName;

		public TodoItemController(ILogger<TodoItemController> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
			_storageConnectionString = _configuration["StorageConnectionString"];
			_tableName = _configuration["TableName"];
		}

		/// <summary>
		/// Retreive all Todo items
		/// </summary>
		/// <remarks>
		/// Sample request:
		/// 
		///     GET /Todo
		///     
		/// </remarks>
		/// <returns>Array of Todo items</returns>
		/// <response code="200">If the items are returned</response>
		[HttpGet]
		public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
		{
			_logger.LogInformation("Getting array of all Todo items");

			var table = TableStorageHelper.GetTableReference(_storageConnectionString, _tableName);

			var query = new TableQuery<TodoItemEntity>();
			var segment = await table.ExecuteQuerySegmentedAsync(query, null);
			var items = segment.Select(TodoItemMapper.ToModel);

			return new OkObjectResult(items);
		}

		/// <summary>
		/// Retreive a specific Todo item
		/// </summary>
		/// <remarks>
		/// Sample request:
		/// 
		///     GET /Todo/6f8c89d7f0824a349e57f7fbb5bab514
		///     
		/// </remarks>
		/// <param name="id" example="6f8c89d7f0824a349e57f7fbb5bab514">id of the Todo item</param>
		/// <returns>The Todo item</returns>
		/// <response code="200">If the item is returned</response>
		/// <response code="404">If the item doesn't exist</response>
		[HttpGet("{id}", Name = "GetTodoItem")]
		[ProducesResponseType(typeof(TodoItem), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<TodoItem>> GetTodoItem(string id)
		{
			_logger.LogInformation($"Getting Todo item, id: {id}");

			var table = TableStorageHelper.GetTableReference(_storageConnectionString, _tableName);

			var findOp = TableOperation.Retrieve<TodoItemEntity>(TodoItemMapper.TablePartitionKey, id);
			var findResult = await table.ExecuteAsync(findOp);
			if (findResult.Result == null)
			{
				return new NotFoundResult();
			}

			var todoEntity = (TodoItemEntity)findResult.Result;

			var todoItem = TodoItemMapper.ToModel(todoEntity);
			return new OkObjectResult(todoItem);
		}

		/// <summary>
		/// Create a new Todo item
		/// </summary>
		/// <remarks>
		/// Sample request:
		/// 
		///     POST /Todo
		///     {
		///         "name": "First Task"
		///     }
		///     
		/// </remarks>
		/// <param name="todoItem">New Todo item to create</param>
		/// <returns>A newly created Todo item</returns>
		/// <response code="201">If the newly created Todo item is returned</response>
		/// <response code="400">If the item is null or item name is empty</response>
		[HttpPost(Name = "CreateTodoItem")]
		[ProducesResponseType(typeof(TodoItem), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<TodoItem>> CreateTodoItem(TodoItem todoItem)
		{
			_logger.LogInformation($"Creating new Todo item");

			if (todoItem == null || string.IsNullOrWhiteSpace(todoItem.Name))
			{
				return new BadRequestResult();
			}

			var table = TableStorageHelper.GetTableReference(_storageConnectionString, _tableName);

			var newTodoItem = new TodoItem { Name = todoItem.Name, IsCompleted = false };
			// Create the InsertOrReplace table operation
			TableOperation insertOp = TableOperation.Insert(TodoItemMapper.ToEntity(newTodoItem));

			// Execute the operation.
			TableResult result = await table.ExecuteAsync(insertOp);
			return CreatedAtAction(nameof(GetTodoItem), new { id = newTodoItem.Id }, newTodoItem);
		}

		/// <summary>
		/// Updates a specific Todo item
		/// </summary>
		/// <remarks>
		/// Sample request:
		/// 
		///     PUT /Todo
		///     {
		///         "isCompleted" : true,
		///         "name": "First Task updated"
		///     }
		///     
		/// </remarks>
		/// <param name="id">id of the Todo item</param>
		/// <param name="todoItem">Todo item to update</param>
		/// <returns>The updated Todo item</returns>
		/// <response code="200">If the updated Todo item is returned</response>
		/// <response code="400">If the item is null or item name is empty</response>
		/// <response code="404">If the item doesn't exist</response>
		[HttpPut("{id}", Name = "UpdateTodoItem")]
		[ProducesResponseType(typeof(TodoItem), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<TodoItem>> UpdateTodoItem(string id, TodoItem todoItem)
		{
			_logger.LogInformation($"Updating Todo item, id: {id}");

			if (todoItem == null || id != todoItem.Id || string.IsNullOrWhiteSpace(todoItem.Name))
			{
				return new BadRequestResult();
			}

			var table = TableStorageHelper.GetTableReference(_storageConnectionString, _tableName);

			var findOp = TableOperation.Retrieve<TodoItemEntity>(TodoItemMapper.TablePartitionKey, id);
			var findResult = await table.ExecuteAsync(findOp);
			if (findResult.Result == null)
			{
				return new NotFoundResult();
			}

			var todoEntity = (TodoItemEntity)findResult.Result;
			todoEntity.Name = todoItem.Name;
			todoEntity.IsCompleted = todoItem.IsCompleted;

			var replaceOp = TableOperation.Replace(todoEntity);
			await table.ExecuteAsync(replaceOp);

			return new OkObjectResult(todoEntity.ToModel());
		}

		/// <summary>
		/// Deletes a specific Todo item
		/// </summary>
		/// <remarks>
		/// Sample request:
		/// 
		///     DELETE /Todo/6f8c89d7f0824a349e57f7fbb5bab514
		///     
		/// </remarks>
		/// <param name="id">id of the Todo item</param>
		/// <returns>Nothing</returns>
		/// <response code="204">If the item is deleted</response>
		/// <response code="404">If the item doesn't exist</response>
		[HttpDelete("{id}", Name = "DeleteTodoItem")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult> DeleteTodoItem(string id)
		{
			_logger.LogInformation($"Deleting Todo item, id: {id}");

			var table = TableStorageHelper.GetTableReference(_storageConnectionString, _tableName);

			var deleteOp = TableOperation.Delete(new TableEntity
			{
				PartitionKey = TodoItemMapper.TablePartitionKey,
				RowKey = id,
				ETag = "*"
			});

			try
			{
				var deleteResult = await table.ExecuteAsync(deleteOp);
			}
			catch (StorageException ex)
			{
				if (ex.RequestInformation.HttpStatusCode == StatusCodes.Status404NotFound)
				{
					return new NotFoundResult();
				}
			}

			return new NoContentResult();
		}
	}
}
