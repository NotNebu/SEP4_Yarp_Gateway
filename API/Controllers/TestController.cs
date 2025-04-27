using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UserServiceProto;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly Greeter.GreeterClient _grpcClient;

    public TestController(Greeter.GreeterClient grpcClient)
    {
        _grpcClient = grpcClient;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var reply = await _grpcClient.SayHelloAsync(new HelloRequest { Name = "Alex" });
        return Ok(new { reply.Message });
    }
}
