using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TestEFCoreMySql;


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
    // | GET ALL MONIES |
    // o----------------o
    [HttpGet]
    [Route("GetAllMonis")]
    public async Task<IEnumerable<Moni>> GetAllMonis()
    {
        using(MoniWatchIContext db = new()) return await db.Monis.ToArrayAsync();
    }


    [HttpGet]
    [Route("GetMoni")]
    public async Task<ActionResult<Moni>> GetMoni(string moniLogin, string moniPwd)
    {
        using (MoniWatchIContext db = new())
        {
            Moni moni = await db.Monis.Where(
                m => m.MoniLogin == moniLogin
            ).FirstOrDefaultAsync();
            
            if (moni is null || /*!BcryptNet.Verify(moniPwd, moni.MoniPwd)*/ moniPwd != moni.MoniPwd)
            {
                return NotFound();
            }
            else
            {
                return Ok(moni);
            }
        }
    }

}