using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebDemo2.Interface;

namespace WebDemo2.Controllers
{
    [Authorize("Permission")]
    [ApiController]
    [Route("api/[controller]")]
    public class RabbitDemoController : ControllerBase
    {
        private readonly IRabbitManager _manager;
        public RabbitDemoController(IRabbitManager manager)
        {
            _manager = manager;
        }

        // GET api/values  
        [HttpGet("PublishDemoMsg")]
        public ActionResult<IEnumerable<string>> PublishDemoMsg()
        {
            // other opreation  

            // if above operation succeed, publish a message to RabbitMQ  

            var num = new System.Random().Next(9000);

            // publish message  
            _manager.Publish(new
            {
                field1 = $"Hello-{num}",
                field2 = $"rabbit-{num}"
            }, "demo.exchange.topic.dotnetcore", "topic", "*.queue.durable.dotnetcore.#");

            return new string[] { "value1", "value2" };
        }
    }
}
