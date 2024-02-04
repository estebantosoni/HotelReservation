using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Xml.Linq;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaNumberAPI")]
    [ApiController]
    public class VillaNumberAPIController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IVillaNumberRepository _villaNumberRepository;
        private readonly IVillaRepository _villaRepository;
        protected APIResponse _apiResponse;

        public VillaNumberAPIController(IMapper mapper, IVillaNumberRepository villaNumberRepository, IVillaRepository villaRepository)
        {
            _mapper = mapper;
            _villaNumberRepository = villaNumberRepository;
            _apiResponse = new();
            _villaRepository = villaRepository;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<VillaNumberDTO>))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetVillaNumbers()
        {

            try
            {
                IEnumerable<VillaNumber> villaList = await _villaNumberRepository.GetAllAsync();
                _apiResponse.Result = _mapper.Map<List<VillaNumberDTO>>(villaList);
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

        [HttpGet("{id:int}",Name = "GetVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetVillaNumber(int id)
        {

            try
            {
                if (id == 0)
                {
                    return BadRequest();
                }
                var villa = await _villaNumberRepository.GetAsync(u => u.VillaNro == id);

                if (villa == null)
                {
                    return NotFound();
                }

                _apiResponse.Result = _mapper.Map<VillaNumberDTO>(villa);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody]VillaNumberCreateDTO createDTO)
        {

            try
            {
                if (await _villaNumberRepository.GetAsync(u => u.VillaNro == createDTO.VillaNro) != null)
                {
                    ModelState.AddModelError("CreateError#1", $"Villa '{createDTO.VillaNro}' already exists. Try create a different villa.");
                    return BadRequest(ModelState);
                }

                if(await _villaRepository.GetAsync(u => u.Id == createDTO.VillaId) == null)
                {
                    ModelState.AddModelError("CreateError#2", $"Villa ID is invalid.");
                    return BadRequest(ModelState);
                }

                if (createDTO == null)
                {
                    return BadRequest();
                }

                VillaNumber villa = _mapper.Map<VillaNumber>(createDTO);

                await _villaNumberRepository.CreateAsync(villa);

                _apiResponse.Result = _mapper.Map<VillaNumberDTO>(villa);
                _apiResponse.StatusCode = HttpStatusCode.Created;

                //To avoid a "System.InvalidOperationException: No route matches the supplied values."
                // add a variable called "id", which matches the GetVillaNumber argument
                return CreatedAtRoute("GetVillaNumber", new { id = villa.VillaNro }, _apiResponse);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> DeleteVillaNumber(int id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest();
                }

                var villa = await _villaNumberRepository.GetAsync(u => u.VillaNro == id);

                if (villa == null)
                {
                    return NotFound();
                }

                await _villaNumberRepository.RemoveAsync(villa);

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int id, [FromBody]VillaNumberUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || id != updateDTO.VillaNro)
                {
                    return BadRequest(_apiResponse.StatusCode = HttpStatusCode.BadGateway);
                }

                if (await _villaRepository.GetAsync(u => u.Id == updateDTO.VillaId) == null)
                {
                    ModelState.AddModelError("CreateError#2", $"Villa ID is invalid.");
                    return BadRequest(ModelState);
                }

                VillaNumber villa = _mapper.Map<VillaNumber>(updateDTO);

                await _villaNumberRepository.UpdateAsync(villa);

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

    }
}
