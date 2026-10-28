using Microsoft.AspNetCore.Mvc;

namespace Nado_Graphics_MS_Backend.Controllers
{
    //One error format for the whole API, RFC 7807 ProblemDetails.
    //"message" is also included so the MVC frontend  reads { message }) keeps working.
    public static class ControllerExtensions
    {
        //-------------------------------------------------------------------------------------------------
        //Builds a ProblemDetails response with the given status code and message
        //Falls back to a generic message if none is supplied
        public static ObjectResult ApiProblem(this ControllerBase controller, int statusCode, string? message)
        {
            message ??= "The request could not be completed.";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = message,
                Detail = message,
                //The path of the request that failed
                Instance = controller.HttpContext.Request.Path
            };

            //Extra "message" field for the frontend, it reads { message } instead of ProblemDetails
            problem.Extensions["message"] = message;

            return new ObjectResult(problem) { StatusCode = statusCode };
        }
        //-------------------------------------------------------------------------------------------------
        //Shortcut for a 400 Bad Request, used when the client sends invalid data
        public static ObjectResult ApiBadRequest(this ControllerBase controller, string? message) =>
            controller.ApiProblem(StatusCodes.Status400BadRequest, message);

        //-------------------------------------------------------------------------------------------------
        //Shortcut for a 409 Conflict, used when the request clashes with the current state of the data        
        public static ObjectResult ApiConflict(this ControllerBase controller, string? message) =>
            controller.ApiProblem(StatusCodes.Status409Conflict, message);
    }
}
//--------------------------------------------------END OF FILE--------------------------------------------