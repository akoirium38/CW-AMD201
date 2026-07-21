using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthService.API.Models;

[Route("api/[controller]")]
[ApiController]
public class OtpCodesController : ControllerBase
{
    private readonly AuthServiceAPIContext _context;
    public OtpCodesController(AuthServiceAPIContext context)
    {
        _context = context;
    }

    // GET: api/OtpCode
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OtpCode>>> GetOtpCode()
    {
        return await _context.OtpCodes.ToListAsync();
    }

    // GET: api/OtpCode/5
    [HttpGet("{id}")]
    public async Task<ActionResult<OtpCode>> GetOtpCode(int id)
    {
        var otpcode = await _context.OtpCodes.FindAsync(id);

        if (otpcode == null)
        {
            return NotFound();
        }

        return otpcode;
    }

    // PUT: api/OtpCode/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutOtpCode(int? id, OtpCode otpcode)
    {
        if (id != otpcode.Id)
        {
            return BadRequest();
        }

        _context.Entry(otpcode).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!OtpCodeExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/OtpCode
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<OtpCode>> PostOtpCode(OtpCode otpcode)
    {
        _context.OtpCodes.Add(otpcode);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetOtpCode", new { id = otpcode.Id }, otpcode);
    }

    // DELETE: api/OtpCode/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOtpCode(int? id)
    {
        var otpcode = await _context.OtpCodes.FindAsync(id);
        if (otpcode == null)
        {
            return NotFound();
        }

        _context.OtpCodes.Remove(otpcode);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool OtpCodeExists(int? id)
    {
        return _context.OtpCodes.Any(e => e.Id == id);
    }
}
