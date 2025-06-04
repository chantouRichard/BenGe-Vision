using Microsoft.AspNetCore.Mvc;
using picture_backend.Entity;
using picture_backend.Services.IServices;

namespace picture_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // 添加控制器路由  
    public class UserController : ControllerBase // 修复：继承自 ControllerBase  
    {
        private readonly IUserLoginService _userLoginService;
        private readonly IUserRegisterService _userRegisterService;
        // 通过构造函数实现依赖注入  
        public UserController(IUserLoginService userLoginService, IUserRegisterService userRegisterService)
        {
            _userLoginService = userLoginService;
            _userRegisterService = userRegisterService;
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login([FromQuery] string username, [FromQuery] string password)
        {
            // 检查用户名和密码是否为空  
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return BadRequest("用户名或密码不能为空");
            }
            // 调用服务层的登录方法  
            var token = await _userLoginService.LoginAsync(username, password);
            if (token == null)
            {
                return Unauthorized("Invalid username or password"); // 返回401 Unauthorized  
            }
            var response = new
            {
                Token = token,
                Username = username,
            };
            return Ok(response); // 返回JWT令牌  
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterEntity request)
        {
            if(string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("用户名或密码不能为空");
            }
            // 调用服务层的注册方法
            var result = await _userRegisterService.RegisterAsync(request.Username, request.Password);
            if (!result)
            {
                return Conflict("用户名存在");
            }
            return Ok("注册成功");
        }
    }
}
