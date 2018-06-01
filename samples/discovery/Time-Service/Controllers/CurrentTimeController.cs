using Microsoft.AspNetCore.Mvc;
using System;

namespace Time_Service.Controllers
{
    [Route("api/[controller]")]
    public class CurrentTimeController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}