using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskService.Dtos;
using TaskService.Exceptions;
using TaskService.Services;

namespace TaskService.Controllers
{
    [Authorize(Policy = "UserPolicy", Roles = "TaskUser")]
    [Route("[controller]")]
    [ApiController]
    public class TaskCategoryController(TaskCategoryService taskCategoryService) : ControllerBase
    {
        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Not Allowed");
            }

            return userId;
        }

        // GET: /TaskCategory
        [HttpGet]
        public async Task<IActionResult> GetTaskCategories()
        {
            var userId = GetUserId();
            var categories = await taskCategoryService.GetCategoriesAsync(userId);
            return Ok(categories);
        }

        // POST: /TaskCategory
        [HttpPost("create")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateTaskCategory dto)
        {
            var userId = GetUserId();
            var category = await taskCategoryService.CreateCategoryAsync(userId, dto);
            return CreatedAtAction(nameof(GetTaskCategories), new { id = category.Id }, category);
        }

        // PUT: /TaskCategory/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] string title)
        {
            var updatedCategory = await taskCategoryService.UpdateCategoryAsync(id, title);
            return Ok(updatedCategory);
        }

        // DELETE: /TaskCategory/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            await taskCategoryService.DeleteCategoryAsync(id);
            return NoContent();
        }
    }
}
