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
using System.Data;
using System.Net;
using System.Xml.Linq;

namespace MagicVilla_VillaAPI.Controllers.v2
{
    //the version route added allows to specify a version in swagger
    [Route("api/v{version:apiVersion}/VillaNumberAPI")]
    [ApiController]

    //specify the default version in the controller
    //this is a dirty approach, the best approach a separate the controller in two
    //[ApiVersion("1.0")]
    //[ApiVersion("2.0")]

    //cleaner approach
    [ApiVersion("2.0")]

    //with deprecated flag
    //this will appear in the request
    //[ApiVersion("2.0",Deprecated = true)]
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

        //if a have another get without parameters is needed to set a name to differentiate gets
        //that name will add in the route of endpoint
        //additionaly, if we have this method with the same name in another controller, this method will have support for many versions
        [HttpGet("GetString")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

    }
}
