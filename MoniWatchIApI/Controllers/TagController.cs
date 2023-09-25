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


    // o--------------o
    // | GET ALL TAGS |
    // o--------------o
    /// <summary>
    /// Get all tags for a specific user with URL: /root/tag/GetAllTags?MoniId={moniId}</br>
    /// Or all tags with URL: /root/tag/GetAllTags
    /// </summary>
    /// <param name="moniId">The id of tag's owner</param>
    /// <returns>An array of tags</returns>
    [HttpGet]
    [Route("GetAllTags")]
    public async Task<IEnumerable<Tag>> GetAllTags(int? moniId)
    {
        using(MoniWatchIContext db = new())
        {
            // Get all tags associated with a specific user
            if(moniId.HasValue)
            {
                return await db.Tags.Where(t => t.MoniId == moniId).ToArrayAsync();
            }
            // Get all tags (I don't see any usecase yet, may remove null forgiving operator on parameter moniId)
            else
            {
                return await db.Tags.ToArrayAsync();
            }
        }
    }

    // o-----------o
    // | GET A TAG |
    // o-----------o
    [HttpGet]
    [Route("GetTag")]
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
    [Route("PostTag")]
    public async Task<ActionResult<Tag>> PostTag(Tag tag)
    {
        using (MoniWatchIContext db = new())
        {
            db.Add(tag);
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllTags), new {id = tag.TagId}, tag);
        }
    }
}