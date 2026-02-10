namespace RavenPost.Api.Dtos;

public record CreateSupplyDto(
    string Name,
    string Category,
    decimal Price
);