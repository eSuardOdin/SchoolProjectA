using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


namespace MoniWatchIApI.Controllers;

[ApiController]
[Route("moni")]
public class MoniController : ControllerBase
{
    private readonly ILogger<MoniController> _logger;
    public MoniController(ILogger<MoniController> logger)
    {
        _logger = logger;
    }

    // o----------------o
    // | GET ALL Monis |
    // o----------------o
    [HttpGet]
    public async Task<IEnumerable<Moni>> GetAllMonis()
    {
        using(MoniWatchIContext db = new()) return await db.Monis.ToArrayAsync();
    }



    // o----------o
    // | GET MONI |
    // o----------o
    /// <summary>
    /// Returns a Moni with matching login and password</br>
    /// Route URL: /root/moni/{login}?moniPwd={pwd}
    /// </summary>
    /// <param name="moniLogin">The Login to search in db</param>
    /// <param name="moniPwd">Password that will be tested against hashed password in db</param>
    /// <returns>Not found or moni object</returns>
    [HttpGet]
    [Route("{moniId}")]
    public async Task<ActionResult<Moni>> GetMoni(string moniPwd, int moniId)
    {
        using (MoniWatchIContext db = new())
        {
            Moni moni = await db.Monis.FindAsync(moniId);
            
            if (moni is null || !BcryptNet.Verify(moniPwd, moni.MoniPwd) /*moniPwd != moni.MoniPwd*/)
            {
                return NotFound();
            }
            else
            {
                return Ok(moni);
            }
        }
    }



    // o-----------o
    // | POST MONI | 
    // o-----------o
    /// <summary>
    /// Adds a Moni to database with an hashed password</br>
    /// With http POST request on URL: /root/moni/PostMoni
    /// </summary>
    /// <param name="moni">Moni object added, will be passed in request as JSON</param>
    /// <returns>The Moni created</returns>
    [HttpPost]
    public async Task<ActionResult<Moni>> PostMoni([FromBody] Moni moni)
    {
        // Pwd encrypt
        moni.MoniPwd = BcryptNet.HashPassword(moni.MoniPwd);
        using (MoniWatchIContext db = new())
        {
            if(moni is null)
            {
                return BadRequest("Bad data provided");
            }
            db.Monis.Add(moni);
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMoni), new {moniLogin = moni.MoniLogin}, moni);

        } 
    }

    // o-------------------o
    // | UPDATE MONI LOGIN |
    // o-------------------o
    // [HttpPatch]
    // [Route("UpdateMoniLogin")]
    // public async Task<ActionResult<Moni>> UpdateMoniLogin(int moniId, string moniLogin)
    // {
    //     using (MoniWatchIContext db = new())
    //     {
    //         Moni moni = await db.Monis.FindAsync(moniId);
    //         if (moni is null)
    //         {
    //             return BadRequest("Moni not found");
    //         }
    //         moni.MoniLogin = moniLogin;
    //         await db.SaveChangesAsync();
    //         return Ok($"Moni login successfully updated: {moni.MoniLogin}");
    //     }
    // }


    // o-----------------o
    // | UPDATE MONI PWD |
    // o-----------------o
    [HttpPatch]
    [Route("{moniId}")]
    public async Task<ActionResult<Moni>> UpdateMoniPwd(int moniId, [FromBody]MoniPatchModel moniPatch)
    {
        using (MoniWatchIContext db = new())
        {
            Moni moni = await db.Monis.FindAsync(moniId);
            if (moni is null)
            {
                return BadRequest("Moni not found");
            }
            // Pwd encrypt
            moni.MoniPwd = BcryptNet.HashPassword(moniPatch.PatchPwd);
            moni.FirstName = moniPatch.PatchFirstName;
            moni.LastName = moniPatch.PatchLastName;
            await db.SaveChangesAsync();
            return Ok($"Moni successfully updated");
        }
    }


    // o-------------o
    // | DELETE MONI |
    // o-------------o
    [HttpDelete]
    [Route("{moniId}")]
    public async Task<ActionResult> DeleteMoni(int moniId)
    {
        using (MoniWatchIContext db = new())
        {
            Moni moni = await db.Monis.FindAsync(moniId);
            if (moni is null)
            {
                return BadRequest("No Monis to delete");
            }
            db.Monis.Remove(moni);
            await db.SaveChangesAsync();
            return Ok($"Moni {moni.MoniLogin} has been deleted");
        }
    }

}