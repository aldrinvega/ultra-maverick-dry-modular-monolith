using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Api.Controllers;

[ApiController]
[Authorize(Policy = Policies.Catalog)]
public sealed class ReferenceDataController : ControllerBase
{
    private readonly IReferenceDataService _data;

    public ReferenceDataController(IReferenceDataService data) => _data = data;

    public sealed record ReferenceBody(string Name, string? Code);
    public sealed record ReferenceUpdateBody(int Id, string Name, string? Code);
    public sealed record ReferenceStatusBody(int Id, bool IsActive);

    [HttpGet("api/RawMaterial/GetAllItemCategory")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetAllItemCategories(CancellationToken ct)
        => List("itemCategory", 1, 200, null, null, ct);

    [HttpGet("api/RawMaterial/GetAllItemCategoryById/{id}")]
    public Task<IActionResult> GetItemCategory(int id, CancellationToken ct)
        => Get("itemCategory", id, ct);

    [HttpGet("api/RawMaterial/GetAllActiveItemCategories")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetActiveItemCategories(CancellationToken ct)
        => List("itemCategory", 1, 200, null, true, ct);

    [HttpGet("api/RawMaterial/GetAllInActiveItemCategories")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetInactiveItemCategories(CancellationToken ct)
        => List("itemCategory", 1, 200, null, false, ct);

    [HttpPost("api/RawMaterial/AddNewItemCategories")]
    public Task<IActionResult> CreateItemCategory([FromBody] ReferenceBody body, CancellationToken ct)
        => Create("itemCategory", body, ct);

    [HttpPut("api/RawMaterial/UpdateItemCategories/{id}")]
    public Task<IActionResult> UpdateItemCategory(int id, [FromBody] ReferenceBody body, CancellationToken ct)
        => Update("itemCategory", id, body, ct);

    [HttpPut("api/RawMaterial/InActiveItemCategory/{id}")]
    public Task<IActionResult> DeactivateItemCategory(int id, CancellationToken ct)
        => SetActive("itemCategory", id, false, ct);

    [HttpPut("api/RawMaterial/ActivateItemCategory/{id}")]
    public Task<IActionResult> ActivateItemCategory(int id, CancellationToken ct)
        => SetActive("itemCategory", id, true, ct);

    [HttpGet("api/RawMaterial/GetAllItemCategoryWithPagination/{status}")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetItemCategoriesPaged(
        bool status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, CancellationToken ct = default)
        => List("itemCategory", page, pageSize, search, status, ct);

    [HttpGet("api/Lot/GetAllLotCategories")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetAllLotCategories(CancellationToken ct)
        => List("lotCategory", 1, 200, null, null, ct);

    [HttpGet("api/Lot/GetLotCategoryById/{id}")]
    public Task<IActionResult> GetLotCategory(int id, CancellationToken ct)
        => Get("lotCategory", id, ct);

    [HttpGet("api/Lot/GetAllActiveLotCategories")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetActiveLotCategories(CancellationToken ct)
        => List("lotCategory", 1, 200, null, true, ct);

    [HttpGet("api/Lot/GetAllInActiveLotCategory")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetInactiveLotCategories(CancellationToken ct)
        => List("lotCategory", 1, 200, null, false, ct);

    [HttpPost("api/Lot/AddNewLotCategory")]
    public Task<IActionResult> CreateLotCategory([FromBody] ReferenceBody body, CancellationToken ct)
        => Create("lotCategory", body, ct);

    [HttpPut("api/Lot/UpdateLotCategory/{id}")]
    public Task<IActionResult> UpdateLotCategory(int id, [FromBody] ReferenceBody body, CancellationToken ct)
        => Update("lotCategory", id, body, ct);

    [HttpPut("api/Lot/InActiveLotCategory/{id}")]
    public Task<IActionResult> DeactivateLotCategory(int id, CancellationToken ct)
        => SetActive("lotCategory", id, false, ct);

    [HttpPut("api/Lot/ActivateLotCategory/{id}")]
    public Task<IActionResult> ActivateLotCategory(int id, CancellationToken ct)
        => SetActive("lotCategory", id, true, ct);

    [HttpGet("api/Lot/GetAllLotCategoryWithPagination/{status}")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetLotCategoriesPaged(
        bool status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, CancellationToken ct = default)
        => List("lotCategory", page, pageSize, search, status, ct);

    [HttpGet("api/Customer/GetAllFarms")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetAllFarms(CancellationToken ct)
        => List("farm", 1, 200, null, null, ct);

    [HttpGet("api/Customer/GetFarmById/{id}")]
    public Task<IActionResult> GetFarm(int id, CancellationToken ct)
        => Get("farm", id, ct);

    [HttpGet("api/Customer/GetAllActiveFarms")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetActiveFarms(CancellationToken ct)
        => List("farm", 1, 200, null, true, ct);

    [HttpGet("api/Customer/GetAllInActiveFarms")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetInactiveFarms(CancellationToken ct)
        => List("farm", 1, 200, null, false, ct);

    [HttpPost("api/Customer/AddNewFarm")]
    public Task<IActionResult> CreateFarm([FromBody] ReferenceBody body, CancellationToken ct)
        => Create("farm", body, ct);

    [HttpPut("api/Customer/UpdateFarm/{id}")]
    public Task<IActionResult> UpdateFarm(int id, [FromBody] ReferenceBody body, CancellationToken ct)
        => Update("farm", id, body, ct);

    [HttpPut("api/Customer/InActiveFarm/{id}")]
    public Task<IActionResult> DeactivateFarm(int id, CancellationToken ct)
        => SetActive("farm", id, false, ct);

    [HttpPut("api/Customer/ActivateFarm/{id}")]
    public Task<IActionResult> ActivateFarm(int id, CancellationToken ct)
        => SetActive("farm", id, true, ct);

    [HttpGet("api/Customer/GetAllFarmWithPagination/{status}")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetFarmsPaged(
        bool status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, CancellationToken ct = default)
        => List("farm", page, pageSize, search, status, ct);

    [HttpGet("api/Transaction/GetAllActiveTransactionName")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetActiveTransactionTypes(CancellationToken ct)
        => List("transactionType", 1, 200, null, true, ct);

    [HttpPost("api/Transaction/AddNewTransaction")]
    public Task<IActionResult> CreateTransactionType([FromBody] ReferenceBody body, CancellationToken ct)
        => Create("transactionType", body, ct);

    [HttpPut("api/Transaction/UpdateTransaction")]
    public Task<IActionResult> UpdateTransactionType([FromBody] ReferenceUpdateBody body, CancellationToken ct)
        => Update("transactionType", body.Id, new ReferenceBody(body.Name, body.Code), ct);

    [HttpPut("api/Transaction/InActiveTransaction/{id}")]
    public Task<IActionResult> DeactivateTransactionType(int id, CancellationToken ct)
        => SetActive("transactionType", id, false, ct);

    [HttpPut("api/Transaction/ActivateTransaction/{id}")]
    public Task<IActionResult> ActivateTransactionType(int id, CancellationToken ct)
        => SetActive("transactionType", id, true, ct);

    [HttpGet("api/Transaction/GetAllTransactionPagination/{status}")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetTransactionTypesPaged(
        bool status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, CancellationToken ct = default)
        => List("transactionType", page, pageSize, search, status, ct);

    [HttpGet("api/LabTestMasterList/GetAllSampleTypes")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetAllSampleTypes(CancellationToken ct)
        => List("sampleType", 1, 200, null, null, ct);

    [HttpGet("api/LabTestMasterList/GetAllSampleTypeByStatus/{status}")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetSampleTypesByStatus(bool status, CancellationToken ct)
        => List("sampleType", 1, 200, null, status, ct);

    [HttpPost("api/LabTestMasterList/AddNewSampleType")]
    public Task<IActionResult> CreateSampleType([FromBody] ReferenceBody body, CancellationToken ct)
        => Create("sampleType", body, ct);

    [HttpGet("api/LabTestMasterList/GetSampleTypeById/{id}")]
    public Task<IActionResult> GetSampleType(int id, CancellationToken ct)
        => Get("sampleType", id, ct);

    [HttpPut("api/LabTestMasterList/UpdateSampleType")]
    public Task<IActionResult> UpdateSampleType([FromBody] ReferenceUpdateBody body, CancellationToken ct)
        => Update("sampleType", body.Id, new ReferenceBody(body.Name, body.Code), ct);

    [HttpGet("api/LabTestMasterList/GetAllSampleTypePagination")]
    public Task<ActionResult<PagedResult<LookupDto>>> GetSampleTypesPaged(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] bool? status = null, CancellationToken ct = default)
        => List("sampleType", page, pageSize, search, status, ct);

    [HttpPut("api/LabTestMasterList/UpdateSampleTypeStatus")]
    public Task<IActionResult> UpdateSampleTypeStatus([FromBody] ReferenceStatusBody body, CancellationToken ct)
        => SetActive("sampleType", body.Id, body.IsActive, ct);

    private async Task<ActionResult<PagedResult<LookupDto>>> List(
        string entity, int page, int pageSize, string? search, bool? isActive, CancellationToken ct)
        => Ok(await _data.ListAsync(entity, page, pageSize, search, isActive, ct));

    private async Task<IActionResult> Get(string entity, int id, CancellationToken ct)
        => (await _data.GetAsync(entity, id, ct)) is { } dto
            ? Ok(dto)
            : NotFound(new ApiResponse(404, "Not found."));

    private async Task<IActionResult> Create(string entity, ReferenceBody body, CancellationToken ct)
    {
        var result = await _data.CreateAsync(entity, body.Name, body.Code, ct);
        return result.Succeeded ? Ok(result.Value) : BadRequest(new ApiResponse(400, result.Error));
    }

    private async Task<IActionResult> Update(string entity, int id, ReferenceBody body, CancellationToken ct)
    {
        var result = await _data.UpdateAsync(entity, id, body.Name, body.Code, ct);
        return result.Succeeded ? NoContent() : BadRequest(new ApiResponse(400, result.Error));
    }

    private async Task<IActionResult> SetActive(string entity, int id, bool isActive, CancellationToken ct)
    {
        var result = await _data.SetActiveAsync(entity, id, isActive, ct);
        return result.Succeeded ? NoContent() : BadRequest(new ApiResponse(400, result.Error));
    }
}
