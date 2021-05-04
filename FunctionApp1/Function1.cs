using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FunctionApp1.Models;
using System.Collections.Generic;
using System.Linq;

namespace FunctionApp1
{

    public static class Function1
    {
        private static readonly List<Todo> Items = new List<Todo>();

        private const string Route = "memorytodo";

        [FunctionName("GetTodos")]
        public static IActionResult GetTodos(
        
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Route)]HttpRequest req, ILogger log)
        {
            try
            {
                log.LogInformation("get todos method invoked");
                Items.Add(new Todo { Id = "1", CreatedTime = DateTime.Now, TaskDescription = "Speech Recognition", IsCompleted = true });
                return new OkObjectResult(Items);
            }
            catch (Exception e)
            {
                log.LogError(e.Message, e);
            }
            return new OkObjectResult(Items);


        }

        [FunctionName("GetTodosById")]
        public static IActionResult GetTodosById(

            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Route+"/{id}")] HttpRequest req, ILogger log,string id)
        {

            var todo = Items.FirstOrDefault(t => t.Id == id);
            try
            {
                
                if (todo == null)
                {
                    return new NotFoundResult();
                }
                return new OkObjectResult(todo);
            }
            catch (Exception e)
            {

                log.LogError(e.Message, e);
            }
            return new OkObjectResult(todo);
        }


        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);
            var todo = new Todo() { TaskDescription = input.TaskDescription };
            try
            {
                log.LogInformation("Creating a new todo list item");
                Items.Add(todo);
                return new OkObjectResult(todo);
            }
            catch (Exception e)
            {

                log.LogError(e.Message, e);
            }
            return new OkObjectResult(todo);


        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodo (
            [HttpTrigger(AuthorizationLevel.Anonymous,"put",Route=Route+"/{id}")]HttpRequest req,ILogger log,string id)
        {
            var todo = Items.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                return new NotFoundResult();
            }

            string requestbody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<TodoUpdateModel>(requestbody);
            todo.IsCompleted = updated.IsCompleted;
            if (!string.IsNullOrEmpty(updated.TaskDescription))
            {
                todo.TaskDescription = updated.TaskDescription;
            }
            return new OkObjectResult(todo);
        }


        [FunctionName("DeleteTodo")]
        public static IActionResult DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route + "/{id}")] HttpRequest req, ILogger log, string id)
        {
            var todo = Items.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                return new NotFoundResult();
            }
            try
            {
                Items.Remove(todo);
                return new OkResult();
            }
            catch(Exception e)
            {
                log.LogError(e.Message, e);
            }
            return new OkResult();
        }




    }
}
