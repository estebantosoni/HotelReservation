using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {

        private readonly ApplicationDBContext _dBContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        
        private string secretKey;
    
        public UserRepository(ApplicationDBContext dbContext,
            IConfiguration config, 
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _dBContext = dbContext;
            secretKey = config.GetValue<string>("ApiSettings:Secret");
            _mapper = mapper;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public bool IsUniqueUser(string username)
        {
            //We dont need LocalUsers anymore
            var user = _dBContext.ApplicationUsers.FirstOrDefault(u => u.UserName == username);
            return user == null;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = _dBContext.ApplicationUsers.FirstOrDefault(u=>u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);

            if (user == null || isValid == false)
            {
                return new LoginResponseDTO()
                {
                    Token = "",
                    User = null
                };
            }

            //generate JWT Token
                
            var roles = await _userManager.GetRolesAsync(user);

            //this var creates the token after the generation of rules for the token
            var tokenHandler = new JwtSecurityTokenHandler();
            
            //encoding secretKey
            var key = Encoding.ASCII.GetBytes(secretKey);

            //token spec and rules
            //when the token is generated when i log in, the token string will contain all the data below
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name,user.UserName.ToString()),
                    //if we have more than one role, we can set a foreach to create a claim for each role
                    new Claim(ClaimTypes.Role,roles.FirstOrDefault())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            //creating token
            var token = tokenHandler.CreateToken(tokenDescriptor);

            LoginResponseDTO loginResponseDTO = new LoginResponseDTO()
            {
                User = _mapper.Map<UserDTO>(user),
                //the token must be serialized because "token" has the type SecurityToken
                Token = tokenHandler.WriteToken(token),
            };
            return loginResponseDTO;
        }

        public async Task<UserDTO> Register(RegisterRequestDTO registerRequestDTO)
        {
            ApplicationUser user= new()
            {
                UserName = registerRequestDTO.UserName,
                Name = registerRequestDTO.Name,
                NormalizedEmail = registerRequestDTO.UserName.ToUpper(),
                Email = registerRequestDTO.UserName,
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registerRequestDTO.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync("admin").GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole("admin"));
                        await _roleManager.CreateAsync(new IdentityRole("customer"));
                    }

                    await _userManager.AddToRoleAsync(user, "admin");
                    var userToReturn = _dBContext.ApplicationUsers.FirstOrDefault(u => u.UserName == registerRequestDTO.UserName);
                    return _mapper.Map<UserDTO>(userToReturn);
                }
            }
            catch (Exception)
            {
                
            }          


            return new UserDTO();
        }
    }
}
