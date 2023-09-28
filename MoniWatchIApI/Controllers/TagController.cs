using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;



namespace MoniWatchIApI.Controllers;


[ApiController]
[Route("tag")]
public class TagController : ControllerBase
{
    private readonly ILogger<TagController> _logger;
    public TagController(ILogger<TagController> logger)
    {
        _logger = logger;
    }

    // o-----------o
    // | GET A TAG |
    // o-----------o
    [HttpGet]
    [Route("{tagId}")]
    /// <summary>
    /// Get tag for a specific tag with URL: /root/tag/GetTag?TagId={tagId}</br>
    /// </summary>
    /// <param name="tagId">Tag's ID</param>
    /// <returns>An array of tags</returns>
    public async Task<ActionResult<Tag>> GetTag(int tagId)
    {
        using (MoniWatchIContext db = new())
        {
            Tag tag = await db.Tags.FindAsync(tagId);
            if (tag is null)
            {
                return NotFound();
            }
            return Ok(tag);
        }
    }

    // o--------------o
    // | POST NEW TAG |
    // o--------------o
    /// <summary>
    /// Posts a tag with URL /root/tag/PostTag 
    /// </summary>
    /// <param name="tag">Tag to post as a JSON</param>
    /// <returns>Status code with tag  object created</returns>       
    [HttpPost]
    public async Task<ActionResult<Tag>> PostTag(Tag tag)
    {
        using (MoniWatchIContext db = new())
        {
            db.Add(tag);
            await db.SaveChangesAsync();
            return Ok();
        }
    }
}