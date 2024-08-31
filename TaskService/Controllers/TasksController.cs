using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using TaskService.DataStorage.Entites;
using TaskService.DataStorage;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace TaskService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController(TaskDbContext context) : ControllerBase
    {
        private readonly TaskDbContext _context = context;

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var userId = GetUserIdFromToken();
            if (userId == null) 
                return NoContent();
            var cacheKey = $"tasks_{userId}";

            //// Попробуйте получить данные из кеша
            //var cachedTasks = await _cache.GetStringAsync(cacheKey);
            //if (cachedTasks != null)
            //{
            //    return Ok(cachedTasks);
            //}

            // Если кеш пуст, получите данные из базы данных
            var tasks = await _context.Tasks.Where(t => t.UserId == userId).ToListAsync();

            //// Сохраните данные в кеш
            //await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(tasks));

            return Ok(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskItem task)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return BadRequest();
            task.UserId = (int)userId; // Установите идентификатор пользователя
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            //// Очистите кеш для этого пользователя
            //await _cache.RemoveAsync($"tasks_{task.UserId}");

            return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, task);
        }

        private int? GetUserIdFromToken()
        {
            // Извлечение идентификатора пользователя из токена
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return null;
            if (!int.TryParse(userIdClaim.Value, out int id))
            {
                return null;
            }
            return id;
        }
    }
}
