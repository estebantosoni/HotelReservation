using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_VillaAPI.Repository
{
    public class UserRepository : IUserRepository
    {

        private readonly ApplicationDBContext _dBContext;
        private string secretKey;
    
        public UserRepository(ApplicationDBContext dbContext, IConfiguration config)
        {
            _dBContext = dbContext;
            secretKey = config.GetValue<string>("ApiSettings:Secret");
        }

        public bool IsUniqueUser(string username)
        {
            var user = _dBContext.LocalUsers.FirstOrDefault(u => u.UserName == username);
            return user == null;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = _dBContext.LocalUsers.FirstOrDefault(u=>u.UserName.ToLower() == loginRequestDTO.UserName.ToLower()
            && u.Password == loginRequestDTO.Password);

            if (user == null)
            {
                return new LoginResponseDTO()
                {
                    Token = "",
                    User = null
                };
            }

            //generate JWT Token
                
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
                    new Claim(ClaimTypes.Name,user.Id.ToString()),
                    new Claim(ClaimTypes.Role,user.Role)
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
                User = user,
                //the token must be serialized because "token" has the type SecurityToken
                Token = tokenHandler.WriteToken(token),
            };
            return loginResponseDTO;
        }

        public async Task<LocalUser> Register(RegisterRequestDTO registerRequestDTO)
        {
            LocalUser localUser= new()
            {
                UserName = registerRequestDTO.UserName,
                Name = registerRequestDTO.Name,
                Role = registerRequestDTO.Role,
                Password = registerRequestDTO.Password,
            };
            await _dBContext.AddAsync(localUser);
            await _dBContext.SaveChangesAsync();

            localUser.Password = "";
            return localUser;
        }
    }
}
