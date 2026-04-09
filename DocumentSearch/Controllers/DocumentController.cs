using DocumentSearch.Models;
using DocumentSearch.Persistance;
using DocumentSearch.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentSearch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public class DocumentController : ControllerBase
    {
        private readonly ILogger<DocumentController> _logger;
        private readonly DocumentDbContext _context;
        private readonly IThaiTokenizerService _tokenizer;

        public DocumentController(ILogger<DocumentController> logger, DocumentDbContext context, IThaiTokenizerService tokenizer)
        {
            _logger = logger;
            _context = context;
            _tokenizer = tokenizer;
        }

        [HttpGet("{search}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync(string search)
        {
            try
            {
                var tokenizedSearch = await _tokenizer.TokenizeAsync(search);

                var documentList = await _context.Documents
                    .Where(d => d.SearchVector.Matches(EF.Functions.PlainToTsQuery("simple", tokenizedSearch)))
                    .Select(d => new
                    {
                        DocumentId = d.Id.ToString(),
                        Rank = d.SearchVector.Rank(EF.Functions.PlainToTsQuery("simple", tokenizedSearch))
                    })
                    .OrderByDescending(d => d.Rank)
                    .ToListAsync();

                if (documentList == null || !documentList.Any())
                    return NotFound("No documents found matching the search criteria");

                return Ok(documentList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching documents with query: {Search}", search);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching documents");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostAsync([FromBody] DocumentIndexingRequest[] request)
        {
            if (request == null || request.Length == 0)
                return BadRequest("Invalid request body");

            try
            {
                var tokenizedContents = await _tokenizer.TokenizeBatchAsync(request.Select(r => r.Content).ToArray());
                var tokenizedFileNames = await _tokenizer.TokenizeBatchAsync(request.Select(r => r.FileName).ToArray());

                var docs = request.Select((r, i) => new Document
                {
                    Id = r.Id,
                    Content = tokenizedContents[i],
                    FileName = tokenizedFileNames[i]
                }).ToArray();

                _context.Documents.AddRange(docs);
                await _context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving document");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while saving document");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            if (!Guid.TryParse(id, out Guid guid))
                return BadRequest("Invalid document ID");

            try
            {
                var document = await _context.Documents.FindAsync(guid);
                if (document == null)
                    return NotFound("Document not found");

                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document with ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the document");
            }
        }
    }
}
