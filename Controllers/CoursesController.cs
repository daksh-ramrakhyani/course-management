using Microsoft.AspNetCore.Mvc;
using CourseManagement.DTOs;
using CourseManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CourseManagement.Controllers;

[ApiController]
[Route("[controller]")]

public class CoursesController : ControllerBase
{

    // DB Context 
    private readonly ICourseService courseService;

    // Constructor
    public CoursesController(ICourseService service)
    {
        this.courseService = service;
    }

    // GET all courses 
    [HttpGet()]

    public async Task<IActionResult> GetCourses()
    {
        var courseResult = await courseService.GetCourses();
        return Ok(courseResult.Data);
    }


    // Get course by Id
    [HttpGet("{id}")]

    public async Task<IActionResult> GetCourseById(Guid id)
    {
        // service 
        var courseResult = await courseService.GetCourseById(id);

        if (courseResult.IsSuccess)
        {
            return Ok(courseResult.Data);
        }
        return NotFound(courseResult.Message);

    }

    [HttpPost]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<ActionResult> CreateCourse(CreateCourseDto dto)
    {

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid teacherId = Guid.Parse(userId!);

        var courseResult = await courseService.CreateCourse(dto, teacherId);
        if (courseResult.IsSuccess)
        {
            return CreatedAtAction(nameof(GetCourseById), new { id = courseResult.Data!.Id }, courseResult.Data);
        }
        if (courseResult.ErrorCode == "DUPLICATE_TITLE")
        {
            return Conflict(courseResult.Message);
        }

        return BadRequest(courseResult.Message);
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<ActionResult> UpdateCourse(Guid id, UpdateCourseDto dto)
    {
        var courseResult = await courseService.UpdateCourse(
            id,
            dto,
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            User.FindFirstValue(ClaimTypes.Role)!
            );


        if (courseResult.IsSuccess)
        {
            return Ok(courseResult.Data);
        }
        if (courseResult.ErrorCode == "DUPLICATE_TITLE")
        {
            return Conflict(courseResult.Message);
        }
        if (courseResult.ErrorCode == "VALIDATION_ERROR")
        {
            return BadRequest(courseResult.Message);
        }

        if (courseResult.ErrorCode == "NOT_AUTHORISED")
        {
            return StatusCode( 403 ,courseResult.Message);
        }

        return NotFound(courseResult.Message);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Teacher, Admin")]
    public async Task<ActionResult> DeleteCourse(Guid id)
    {
        var courseResult = await courseService.DeleteCourse(
            id,
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            User.FindFirstValue(ClaimTypes.Role)!
        );

        if (courseResult.IsSuccess)
        {
            return NoContent();
        }
        return NotFound(courseResult.Message);
    }
}
