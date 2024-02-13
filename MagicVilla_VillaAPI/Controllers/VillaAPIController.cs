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
using System.Xml.Linq;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
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
        public async Task<ActionResult<APIResponse>> GetVillas()
        {

            try
            {
                IEnumerable<Villa> villaList = await _villaRepository.GetAllAsync();
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

        [HttpGet("{id:int}",Name = "GetVilla")]
        [Authorize(Roles = "admin")]
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
                    return BadRequest();
                }
                var villa = await _villaRepository.GetAsync(u => u.Id == id);

                if (villa == null)
                {
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
        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody]VillaCreateDTO createDTO)
        {

            try
            {
                if (await _villaRepository.GetAsync(u => u.Name.ToLower() == createDTO.Name) != null)
                {
                    ModelState.AddModelError("CreateError#1", $"Villa '{createDTO.Name}' already exists. Try create a different villa.");
                    return BadRequest(ModelState);
                }

                if (createDTO == null)
                {
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
                    return BadRequest();
                }

                var villa = await _villaRepository.GetAsync(u => u.Id == id);

                if (villa == null)
                {
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
        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody]VillaUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || id != updateDTO.Id)
                {
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
        //only for learning
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

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            return NoContent();
        }


    }
}
