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
using Microsoft.Azure.WebJobs.Extensions.Storage;
using System.Text;

namespace FunctionApp1
{

    public static class Function1
    {
        
        private const string Route = "memorytodo";

        static List<Todo> Items = Todo.GetList();


        //Http Trigger with Queue storage
        [FunctionName("GetName")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Queue("outputqueue"), StorageAccount("AzureWebJobsStorage")] ICollector<string> msg,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;
            msg.Add($"Name passed is:{name}");
            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        
        //HttpTrigger of type "GET"
        [FunctionName("GetTodos")]
        public static IActionResult GetTodos(
        
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Route)]HttpRequest req, ILogger log)

        {

            try
            {
                log.LogInformation("get todos method invoked");
                return new OkObjectResult(Items);
            }
            catch (Exception e)
            {
                log.LogError(e.Message, e);
            }
            return new OkObjectResult(Items);


        }
        //HttpTrigger of type "GET BY ID"

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

        //HttpTrigger of type "POST"

        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<Todo>(requestBody);
            var todo = new Todo() { Id = input.Id, CreatedTime = input.CreatedTime, TaskDescription = input.TaskDescription, IsCompleted = input.IsCompleted };
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

        //HttpTrigger of type "PUT"

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

        //HttpTrigger of type "DELETE"

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

        //Time Trigger 

        [FunctionName("TimeTriggerFunction")]
        public static void TimeTriggerFunction([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Timer trigger executed at: {DateTime.Now}");

            var text = string.Empty;

            for (int i = 0; i < 4000; i++)
            {
                text = text + "-" + DateTime.Now;
            }

            if (text.Length < 10)
            {
                log.LogInformation($"dummy text");
            }

            log.LogInformation($"Function processed message with length: {text.Length}");
        }

        //Blob Trigger with Azure Function
        //We add txt file by selecting and put in dev folder
        [FunctionName("BlobFunction")]
        public static void BlobFunction([BlobTrigger("dev/{filename}", Connection = "AzureWebJobsStorage")] Stream myblob,string filename, ILogger log,string blobTrigger)
        {
            try
            {
                StreamReader sm = new StreamReader(myblob);
                log.LogInformation($"c# blob trigger function processed blob \n File Name:{filename} \n Size:{myblob.Length} Bytes \n Content:{sm.ReadToEnd()} \n Path:{blobTrigger} ");
            }
            catch(Exception e)
            {
                log.LogError("Error:", e.Message);

            }
        }


        //Blob Input Binding with Azure Function
        //Type txt file in queue and check whether data is printed in console or not.
        [FunctionName("BlobInputBinding")]

        public static void BlobInputBinding([QueueTrigger("myqueueitems",Connection = "AzureWebJobsStorage")] string myqueueItem,
            [Blob("dev/{queueTrigger}",FileAccess.Read,Connection = "AzureWebJobsStorage")] Stream s, ILogger log)
        {
            try
            {
                StreamReader sm = new StreamReader(s);
                log.LogInformation($" \n File Name:{myqueueItem} \n Size:{s.Length} Bytes \n Content:{sm.ReadToEnd()}  ");
            }
            catch (Exception e)
            {
                log.LogError("Error:", e.Message);
                
            }
            

        }

        //Blob Output Binding with Azure Function
        // Write msg in myqueueitems and that is reflected back in blob.
        [FunctionName("BlobOutputBinding")]

        public static void BlobOutputBinding([QueueTrigger("myqueueitems", Connection = "AzureWebJobsStorage")] string myqueueItem,
            [Blob("dev/abc.txt", FileAccess.Write, Connection = "AzureWebJobsStorage")] Stream outblob, ILogger log)
        {
            try
            {
                log.LogInformation($"c# Queue trigger function processed:{myqueueItem}");
                outblob.Write(Encoding.ASCII.GetBytes(myqueueItem));
            }
            catch (Exception e)
            {
                log.LogError("Error:", e.Message);
            }


        }


        //Queue trigger
        //adding msg in new queue
        [FunctionName("AddingMsgToQueue")]
        public static void AddingMsgToQueue(
            [QueueTrigger("mynewqueue",Connection = "AzureWebJobsStorage")] string mynewQueue, ILogger log,string Id)
        {
            try
            {
                log.LogInformation($"C# queue trigger function processed:{mynewQueue}\n Id:{Id}");

            }
            catch (Exception e)
            {
                log.LogError("Error:", e.Message);
            }

        }

    }


}
