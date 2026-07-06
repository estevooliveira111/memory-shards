using System.ComponentModel.DataAnnotations;
using Api.DTOs;
using Api.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/messages")]
[Produces("application/json")]
public sealed class MessagesController(
    IMessageService messageService,
    IValidator<CreateMessageRequest> createValidator,
    IValidator<GetMessageRequest> getValidator) : ControllerBase
{
    /// <summary>Creates a new temporary message.</summary>
    /// <remarks>
    /// Returns a shareable link with the specified expiration period.
    /// If a password is supplied, the message will be encrypted and can only
    /// be read by providing the correct password.
    /// </remarks>
    /// <response code="201">Message created successfully.</response>
    /// <response code="422">Validation error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateMessageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMessageRequest request,
        CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);
        var response = await messageService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetBySlug), new { slug = response.Slug }, response);
    }

    /// <summary>Retrieves a message by its slug.</summary>
    /// <remarks>
    /// For unprotected messages, send an empty body or omit the body entirely.
    /// For password-protected messages, include the password in the request body.
    /// </remarks>
    /// <response code="200">Message retrieved successfully.</response>
    /// <response code="401">Incorrect or missing password.</response>
    /// <response code="404">Message not found.</response>
    /// <response code="410">Message has expired.</response>
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(GetMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public async Task<IActionResult> GetBySlug(
        [FromRoute] string slug,
        [FromHeader(Name = "X-Password")] string? password,
        CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(password))
        {
            await getValidator.ValidateAndThrowAsync(new GetMessageRequest { Password = password }, ct);
        }

        var response = await messageService.GetBySlugAsync(slug, password, ct);
        return Ok(response);
    }

    /// <summary>Deletes a message by its slug (admin operation).</summary>
    /// <response code="204">Message deleted successfully.</response>
    /// <response code="404">Message not found.</response>
    [HttpDelete("{slug}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] string slug,
        CancellationToken ct)
    {
        await messageService.DeleteBySlugAsync(slug, ct);
        return NoContent();
    }
}
