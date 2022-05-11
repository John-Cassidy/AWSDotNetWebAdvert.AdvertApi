using AdvertApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using AdvertApi.Models;
using Amazon.SimpleNotificationService;
using AdvertApi.Models.Messages;
using System.Text.Json;

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
                await RaiseAdvertConfirmedMessage(model);                
        } catch(KeyNotFoundException) {
                return new NotFoundResult();
        } catch(Exception ex) {
                return StatusCode(500, ex.Message);
        }
            return new OkResult();
        }

        private async Task RaiseAdvertConfirmedMessage(ConfirmAdvertModel model) {
            var topicArn = Configuration.GetValue<string>("AWS:TopicArn");
            var dbModel = await _advertStorageService.GetByIdAsync(model.Id);

            using (var client = new AmazonSimpleNotificationServiceClient()) {
                var message = new AdvertConfirmedMessage {
                    Id = model.Id,
                    Title = dbModel.Title
                };

                var messageJson = JsonSerializer.Serialize(message);
                await client.PublishAsync(topicArn, messageJson);
            }
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Get(string id) {
            try {
                var advert = await _advertStorageService.GetByIdAsync(id);
                return new JsonResult(advert);
            } catch (KeyNotFoundException) {
                return new NotFoundResult();
            } catch (Exception) {
                return new StatusCodeResult(500);
            }
        }

        [HttpGet]
        [Route("all")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> All() {
            return new JsonResult(await _advertStorageService.GetAllAsync());
        }
    }
}
