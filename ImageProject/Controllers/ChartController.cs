using ImageProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProject.Controllers
{
    public class ChartController : Controller
    {
        private readonly ILogger<ChartController> _logger;
        private readonly ApplicationContext _applicationContext;

        public ChartController(ILogger<ChartController> logger, ApplicationContext applicationContext)
        {
            _logger = logger;
            _applicationContext = applicationContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int Id)
        {
            return View(await _applicationContext.СonstituentСolors.Where(u => u.Image.Id == Id).ToListAsync());
        }
    }
}
