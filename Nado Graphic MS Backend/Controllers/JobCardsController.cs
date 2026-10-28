using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Security;
using Nado_Graphics_MS_Backend.Services;

namespace Nado_Graphics_MS_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    //Any logged-in user can use these endpoints (all roles work with job cards). 
    [Authorize]
    public class JobCardsController : ControllerBase
    {
        //The below line stores the JobCardService. 
        private readonly JobCardService _jobCardService;

        //The constructor receives the JobCardService using dependency injection. 
        public JobCardsController(JobCardService jobCardService)
        {
            _jobCardService = jobCardService;
        }

        //----------------------------------------------------------------------------------// 

        //This gets all job cards from the database. 
        [HttpGet]
        public async Task<IActionResult> GetJobCards()
        {
            //This gets the job cards from the service. 
            var jobCards = await _jobCardService.GetJobCardsAsync();

            //This returns the job cards to the frontend. 
            return Ok(jobCards);
        }
        //--------------------------------------------------------------------------------------------// 

        [HttpGet("archived")]
        public async Task<IActionResult> GetArchivedJobCards()
        {
            var jobCards = await _jobCardService.GetArchivedJobCardsAsync();

            return Ok(jobCards);
        }
        //----------------------------------------------------------------------------------// 

        //This gets one Job Card using its ID. 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobCard(int id)
        {
            //This gets the selected Job Card from the service. 
            var jobCard = await _jobCardService.GetJobCardAsync(id);

            //This checks whether the Job Card does not exist. 
            if (jobCard == null)
            {
                return NotFound();
            }

            //This returns the Job Card. 
            return Ok(jobCard);
        }
        //----------------------------------------------------------------------------------// 

        //This gets all suppliers that can be assigned to a Job Card. 
        [HttpGet("suppliers")]
        public async Task<IActionResult> GetSuppliers()
        {
            var suppliers = await _jobCardService.GetSuppliersAsync();

            return Ok(suppliers);
        }
        //----------------------------------------------------------------------------------// 

        //This assigns a supplier to a Job Card. 
        [HttpPut("{id}/supplier")]
        public async Task<IActionResult> AssignSupplier(int id, [FromBody] int supplierId)
        {
            var success = await _jobCardService.AssignSupplierAsync(id, supplierId);

            if (!success)
            {
                return this.ApiBadRequest("Unable to assign the supplier.");
            }

            return NoContent();
        }
        //----------------------------------------------------------------------------------// 

        //This updates an existing Job Card. 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJobCard(int id, JobCard jobCard)
        {
            //This sends the updated Job Card to the service. 
            var success = await _jobCardService.UpdateJobCardAsync(id, jobCard);

            //This checks whether the update failed. 
            if (!success)
            {
                return this.ApiBadRequest("Unable to update the job card.");
            }

            //This returns a successful response. 
            return NoContent();
        }
        //---------------------------------------------------------------------------------------------------// 

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var success = await _jobCardService.UpdateStatusAsync(id, status);

            if (!success)
            {
                return this.ApiBadRequest("Unable to update the job card status.");
            }

            return NoContent();
        }
        //-------------------------------------------------------------------------------------------------------// 

        [HttpPut("{id}/payment-status")]
        [Authorize(Roles = AppRoles.ManagerOrAdmin)]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] string paymentStatus)
        {
            var success = await _jobCardService.UpdatePaymentStatusAsync(id, paymentStatus);

            if (!success)
            {
                return this.ApiBadRequest("Unable to update the payment status.");
            }

            return NoContent();
        }
    }
}
