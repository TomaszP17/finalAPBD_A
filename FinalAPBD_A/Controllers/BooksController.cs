using FinalAPBD_A.DTOs;
using FinalAPBD_A.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FinalAPBD_A.Controllers;

[ApiController]
[Route("/api/books")]
public class BooksController(IDbService db) : ControllerBase
{
    [HttpGet("{id:int}/genres")]
    public async Task<IActionResult> GetBookByGenres(int id)
    {
        var result = await db.GetBookById(id);
        if (result == null) return NotFound($"The book with id: {id} is not exists");
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddBook(AddBookDto bookDto, IValidator<AddBookDto> validator)
    {
        var validate = await validator.ValidateAsync(bookDto);

        if (!validate.IsValid)
        {
            return ValidationProblem();
        }

        var result = await db.AddBookAsync(bookDto);
        if (result == -1)
        {
            return NoContent();
        }

        var createdBook = await db.GetBookById(result);
        return Created($"/api/books/{result}/genres", createdBook);
    }
}