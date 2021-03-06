using System.Threading.Tasks;
using K8sJanitor.WebApi.Infrastructure.Messaging;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace K8sJanitor.WebApi.Controllers
{
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly  IEventDispatcher _eventDispatcher;

        public EventsController(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }

        [HttpPost("")]
        public async Task AddEvent([FromBody] dynamic jObject)
        {
            var eventJson = jObject.ToString();
            Log.Warning("Deprecated call to /api/events with contents {eventJson}", eventJson);
            await _eventDispatcher.Send(eventJson);
        }
    }
}