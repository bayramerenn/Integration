using IntegrationDistributed.API.Common;
using IntegrationDistributed.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationDistributed.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IntegrationsController : ControllerBase
    {
        private readonly ItemIntegrationService _itemIntegrationService;

        public IntegrationsController(ItemIntegrationService itemIntegrationService)
        {
            _itemIntegrationService = itemIntegrationService;
        }

        [HttpPost]
        public async Task<IActionResult> Add(ItemDto dto, CancellationToken cancellationToken)
        {
            await Task.Delay(2000, cancellationToken);

            var result = await _itemIntegrationService.SaveItemAsync(dto.Content, cancellationToken);

            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(CancellationToken cancellationToken)
        {
            await _itemIntegrationService.DeleteAsync(cancellationToken);

            return NoContent();
        }
    }
}