using System;
using System.Collections.Generic;
using System.Text;

namespace Softveda.Todo.Shared.Models
{
	/// <summary>
	/// The Todo item model
	/// </summary>
	public class TodoItem
	{
		/// <summary>
		/// The id of the item
		/// </summary>
		/// <example>6f8c89d7f0824a349e57f7fbb5bab514</example>
		public string Id { get; set; } = Guid.NewGuid().ToString("n");

		/// <summary>
		/// The name of the item
		/// </summary>
		/// <example>My First Task</example>
		public string Name { get; set; }

		/// <summary>
		/// Completion status of the item
		/// </summary>
		/// <example>true</example>
		/// 
		public bool IsCompleted { get; set; }
	}
}
