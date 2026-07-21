using System.Net;
using CqrsExample.Dtos;
using CqrsExample.Features.Products.Commands;
using CqrsExample.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CqrsExample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductCreateDto productCreateDto)
    {
        await _mediator.Send(new CreateProductCommand(productCreateDto));
        return StatusCode((int)HttpStatusCode.Created);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _mediator.Send(new GetAllProductsQuery());
        return Ok(products);
    }
}