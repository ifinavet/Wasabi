using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Wasabi.Controllers.API;

[ApiController]
[Route("/api/member/updateMember")]
public class UpdateMembers : Controller
{
    private readonly IMemberService _memberService;

    public UpdateMembers(
        IMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        Console.WriteLine("start");
        long start = DateTime.Now.Ticks;
        IEnumerable<IMember> members = _memberService.GetAllMembers().OrderBy(x => x.LastLoginDate);

        foreach (IMember member in members)
        {
            if (member.LastLoginDate < DateTime.Now.AddYears(-1))
                _memberService.Delete(member);
        }

        var output = members
            .Select(x => new { x.Name, x.LastLoginDate, x.IsLockedOut })
            .OrderByDescending(x => x.LastLoginDate).ToArray();

        Console.WriteLine(output.Length);

        Console.WriteLine(DateTime.Now.Ticks - start);
        Console.WriteLine("Finished");
        return Ok(output);
    }
}