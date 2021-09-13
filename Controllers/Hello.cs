using Microsoft.AspNetCore.Mvc;

namespace Emille.Controllers
{
    [ApiController]
    [Route("/")]
    public class Hello : ControllerBase
    {
        [HttpGet]
        public string Greeting() => "Hello, yes, you reached Emille.";
    }
}