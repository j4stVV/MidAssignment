namespace Lib.Application.Dtos.BookDTOs;

public class UpdateBookDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public int Quantity { get; set; }
    public int Available { get; set; }
    public Guid CategoryId { get; set; }
}
