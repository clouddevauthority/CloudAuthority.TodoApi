using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Softveda.Todo.Shared.Models;

namespace Softveda.Todo.Api.InMemory.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TodoItemController : ControllerBase
	{
		static ConcurrentDictionary<string, TodoItem> _todoItemsCollection
			= new ConcurrentDictionary<string, TodoItem>();

		private readonly ILogger<TodoItemController> _logger;
		private readonly IConfiguration _configuration;

		public TodoItemController(ILogger<TodoItemController> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
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
		public ActionResult<IEnumerable<TodoItem>> GetTodoItems()
		{
			_logger.LogInformation("Getting array of all Todo items");

			var items = _todoItemsCollection.Values;
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
		public ActionResult<TodoItem> GetTodoItem(string id)
		{
			_logger.LogInformation($"Getting Todo item, id: {id}");

			bool result = _todoItemsCollection.TryGetValue(id, out var todoItem);
			if (result)
			{
				return new OkObjectResult(todoItem);
			}
			return new NotFoundResult();
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
		public ActionResult<TodoItem> CreateTodoItem(TodoItem todoItem)
		{
			_logger.LogInformation($"Creating new Todo item");

			if (todoItem is null || string.IsNullOrWhiteSpace(todoItem.Name))
			{
				return new BadRequestResult();
			}

			todoItem.Id = Guid.NewGuid().ToString("n");
			_todoItemsCollection.TryAdd(todoItem.Id, todoItem);
			return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
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
		public ActionResult<TodoItem> UpdateTodoItem(string id, TodoItem todoItem)
		{
			_logger.LogInformation($"Updating Todo item, id: {id}");

			if (todoItem is null || 
				id != todoItem.Id || 
				string.IsNullOrWhiteSpace(todoItem.Name))
			{
				return new BadRequestResult();
			}

			if (!_todoItemsCollection.ContainsKey(id))
			{
				return new NotFoundResult();
			}

			_todoItemsCollection[id] = todoItem;

			return new OkObjectResult(todoItem);
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
		public ActionResult DeleteTodoItem(string id)
		{
			_logger.LogInformation($"Deleting Todo item, id: {id}");

			if (!_todoItemsCollection.ContainsKey(id))
			{
				return new NotFoundResult();
			}

			_todoItemsCollection.TryRemove(id, out var _);

			return new NoContentResult();
		}
	}
}