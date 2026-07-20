using System.Net;
using CqrsExample.Features.Products.Commands;
using CqrsExample.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CqrsExample.Controllers;

[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create()
    {
        await _mediator.Send(new CreateProductCommand());
        return StatusCode((int)HttpStatusCode.Created);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _mediator.Send(new GetAllProductsQuery());
        return Ok(products);
    }
}