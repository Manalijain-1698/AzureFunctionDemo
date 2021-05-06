using System;
using System.Collections.Generic;
using System.Text;

namespace FunctionApp1.Models
{
    public class Todo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }

        public static List<Todo> GetList()
        {
            List<Todo> todolist = new List<Todo>();
            todolist.Add(new Todo { Id = "1", CreatedTime = DateTime.Now, TaskDescription = "Face Detection", IsCompleted = true });
            return todolist;
        }

    }

   

    
}
