using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using System.Xml.Linq;

namespace MagicVilla_VillaAPI.Controllers.v1
{
    [Route("api/v{version:apiVersion}/VillaAPI")]
    [ApiVersion("1.0")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IVillaRepository _villaRepository;
        protected APIResponse _apiResponse;

        public VillaAPIController(IMapper mapper, IVillaRepository villaRepository)
        {
            _mapper = mapper;
            _villaRepository = villaRepository;
            _apiResponse = new();
        }

        [HttpGet]
        //Example:
        //if we use Authorize for our API, it won't be possible to see the content, even if we are logged
        //we need to pass the generated token to allow the access
        //[Authorize]
        [ProducesResponseType(200, Type = typeof(IEnumerable<VillaDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<APIResponse>> GetVillas(
            [FromQuery(Name = "filterOccupancy")] int? ocuppancy,
            [FromQuery(Name = "searchName")] string? search,
            //also we add pagination, that allow us to see fragmented data in pages
            //pageSize is the max unit of records in one page / pageNumber is the current page
            int pageSize = 0, int pageNumber = 1)
        {
            try
            {
                //we can filter with a property that was took with the annotation FromQuery and test it in swagger
                //filter allow us to filter data without calling query methods
                //also we can use search to receive some data without going to the database
                IEnumerable<Villa> villaList;

                if (ocuppancy > 0)
                {
                    villaList = await _villaRepository.GetAllAsync(u => u.Occupancy == ocuppancy,
                        pageSize:pageSize, pageNumber:pageNumber);
                }
                else
                {
                    villaList = await _villaRepository.GetAllAsync(pageSize: pageSize, pageNumber: pageNumber);
                }
                if (!string.IsNullOrEmpty(search))
                {
                    villaList = villaList.Where(u => u.Name.ToLower().Contains(search));
                }

                //dont delete
                //IEnumerable<Villa> villaList = await _villaRepository.GetAllAsync();

                Pagination pagination = new()
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                };

                //this line add a visual pagination to swagger when we received the response
                //pagination will add in the header
                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));

                _apiResponse.Result = _mapper.Map<List<VillaDTO>>(villaList);
                _apiResponse.StatusCode = HttpStatusCode.OK;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {

                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>()
                {
                    ex.Message
                };
            }
            return _apiResponse;


        }

        [HttpGet("{id:int}", Name = "GetVilla")]
        [Authorize(Roles = "admin")]
        //Set cache duration.
        //When is inside of the time period, resonse is catched on the cache and doesnt pass to the endpoint
        //there are many options, like where store the data and other options
        //[ResponseCache(Duration = 10, Location = ResponseCacheLocation.None, NoStore = true)]
        
        //if we have a cache profile, we can call the profile with your default name
        [ResponseCache(CacheProfileName = "Default30")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetVilla(int id)
        {

            try
            {
                if (id == 0)
                {
                    _apiResponse.IsSuccess = false;
                    return BadRequest();
                }
                var villa = await _villaRepository.GetAsync(u => u.Id == id);

                if (villa == null)
                {
                    _apiResponse.IsSuccess = false;
                    return NotFound();
                }

                _apiResponse.Result = _mapper.Map<VillaDTO>(villa);
                _apiResponse.StatusCode = HttpStatusCode.OK;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {

                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>()
                {
                    ex.Message
                };
            }
            return _apiResponse;

        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDTO createDTO)
        {

            try
            {
                if (await _villaRepository.GetAsync(u => u.Name.ToLower() == createDTO.Name) != null)
                {
                    ModelState.AddModelError("CreateError#1", $"Villa '{createDTO.Name}' already exists. Try create a different villa.");
                    _apiResponse.IsSuccess = false;
                    return BadRequest(ModelState);
                }

                if (createDTO == null)
                {
                    _apiResponse.IsSuccess = false;
                    return BadRequest();
                }

                Villa villa = _mapper.Map<Villa>(createDTO);

                await _villaRepository.CreateAsync(villa);

                _apiResponse.Result = _mapper.Map<VillaDTO>(villa);
                _apiResponse.StatusCode = HttpStatusCode.Created;

                return CreatedAtRoute("GetVilla", new { villa.Id }, _apiResponse);
            }
            catch (Exception ex)
            {

                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>()
                {
                    ex.Message
                };
            }
            return _apiResponse;

        }

        [HttpDelete("{id:int}")]
        //if we log in in swagger like admins, some endpoints wont work because the role
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
        {
            try
            {
                if (id == 0)
                {
                    _apiResponse.IsSuccess = false;
                    return BadRequest();
                }

                var villa = await _villaRepository.GetAsync(u => u.Id == id);

                if (villa == null)
                {
                    _apiResponse.IsSuccess = false;
                    return NotFound();
                }

                await _villaRepository.RemoveAsync(villa);

                _apiResponse.StatusCode = HttpStatusCode.NoContent;
                _apiResponse.IsSuccess = true;

                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {

                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>()
                {
                    ex.Message
                };
            }
            return _apiResponse;
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || id != updateDTO.Id)
                {
                    _apiResponse.IsSuccess = false;
                    return BadRequest(_apiResponse.StatusCode = HttpStatusCode.BadGateway);
                }

                Villa villa = _mapper.Map<Villa>(updateDTO);

                await _villaRepository.UpdateAsync(villa);

                _apiResponse.StatusCode = HttpStatusCode.NoContent;
                _apiResponse.IsSuccess = true;

                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>()
                {
                    ex.Message
                };
            }
            return _apiResponse;
        }

        //Deprecated
        //only for learning how works patch
        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            if (patchDTO == null || id == 0)
            {
                return BadRequest();
            }

            var villa = await _villaRepository.GetAsync(u => u.Id == id, tracked: false);

            //the document expects a VillaDTO, that is why the conversion is necessary
            VillaUpdateDTO updateDTO = _mapper.Map<VillaUpdateDTO>(villa);

            if (villa == null)
            {
                return NotFound();
            }

            patchDTO.ApplyTo(updateDTO, ModelState);

            Villa model = _mapper.Map<Villa>(updateDTO);

            await _villaRepository.UpdateAsync(model);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return NoContent();
        }


    }
}
