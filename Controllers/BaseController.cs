using Microsoft.AspNetCore.Mvc;

namespace PlanIt.Controllers
{
    public class BaseController : ControllerBase
    {
        protected bool TryGetUserId(out int? userId)
        {
            var result = HttpContext.Items.TryGetValue("user", out var user);

            if (result)
            {
                userId = (int)user;
                return true;
            }
            else
            {
                userId = null;
                return false;
            }
        }
    }
}
