using AdvertApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using AdvertApi.Models;

namespace AdvertApi.Controllers {
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AdvertController : ControllerBase {

        private readonly IAdvertStorageService _advertStorageService;
        public IConfiguration Configuration { get; }

        public AdvertController(IAdvertStorageService advertStorageService, IConfiguration configuration) {
            _advertStorageService = advertStorageService;
            Configuration = configuration;
        }

        [HttpPost]
        [Route("Create")]
        [ProducesResponseType(404)]
        [ProducesResponseType(201, Type = typeof(CreateAdvertResponse))]
        public async Task<IActionResult> Create(AdvertModel model) {
            string recordId;
            try {
                recordId = await _advertStorageService.AddAsync(model);
            } catch(KeyNotFoundException) {
                return new NotFoundResult();
            } catch(Exception ex) {
                return StatusCode(500, ex.Message);
            }

            return StatusCode(201, new CreateAdvertResponse { Id = recordId });
        }

        [HttpPut]
        [Route("Confirm")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Confirm(ConfirmAdvertModel model) {
        try {
                await _advertStorageService.ConfirmAsync(model);
                
        } catch(KeyNotFoundException) {
                return new NotFoundResult();
        } catch(Exception ex) {
                return StatusCode(500, ex.Message);
        }
            return new OkResult();
        }

    }
}
