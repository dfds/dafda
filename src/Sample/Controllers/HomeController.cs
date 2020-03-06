using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sample.Infrastructure.Persistence;

namespace Sample.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly Transactional _transactional;

        public HomeController(Transactional transactional)
        {
            _transactional = transactional;
        }

        [Route("/")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            await _transactional.Execute<ApplicationService>(service => service.Process(), cancellationToken);

            return Ok();
        }
    }
}