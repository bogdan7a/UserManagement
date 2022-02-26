using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.JsonPatch;
using UserManagement.Data.Repository;
using UserManagement.Dtos.User;
using UserManagement.Models;

namespace UserManagement.Data.Services.User
{
    public class UserService : BaseRepository<UserModel, UserRead, UserRegister, UserUpdate>, IUserService
    {
        private readonly IConfiguration _configuration;

        public UserService(AppDbContext context, IMapper mapper, IConfiguration configuration) : base(context, mapper)
        {
            _configuration = configuration;
        }


        // Public methods
        public async Task<TokenModel> Login([FromBody] UserLogin user)
        {
            var userFromRepo = await _context.Users.AsQueryable()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == user.Email.ToLower());
            if (userFromRepo == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (!VerifyPasswordHash(user.Password, userFromRepo.PasswordHash, userFromRepo.PasswordSalt))
            {
                throw new BadHttpRequestException("Wrong password");
            }

            var accessToken = CreateAccessToken(userFromRepo);
            var refreshToken = GenerateRefreshToken();
            userFromRepo.RefreshToken = refreshToken;
            userFromRepo.RefreshTokenExpireTime = DateTime.Now.AddDays(7);
            await _context.SaveChangesAsync();

            return new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        public async Task Register(UserRegister user)
        {
            var userFromDb = await _context.Users.AsQueryable()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == user.Email.ToLower());
            if (userFromDb != null)
            {
                throw new KeyNotFoundException($"A user with {user.Email} email address already exist");
            }

            var userModel = _mapper.Map<UserModel>(user);
            CreatePasswordHash(user.Password, user.PasswordConfirmation, out byte[] passwordHash, out byte[] passwordSalt);
            userModel.Email = user.Email;
            userModel.PasswordHash = passwordHash;
            userModel.PasswordSalt = passwordSalt;
            await _context.Users.AddAsync(userModel);
            await _context.SaveChangesAsync();
        }
        public async Task ChangeUserRole(int id, Helpers.UserRole role)
        {
            var userFromDb = await _context.Users.AsQueryable()
                .FirstOrDefaultAsync(u => u.Id == id);
            if (userFromDb == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            var managers = _context.Users.AsQueryable().Where(x => x.Role == "Manager");
            var roleString = new StringBuilder();
            if (managers.Count() == 1)
            {
                if (userFromDb.Role == "Manager")
                {
                    throw new BadHttpRequestException(
                        "You are the only manager and you can't change your role until there is alt least one more manager");
                }
            }

            switch (role)
            {
                case Helpers.UserRole.Employee:
                    roleString.Clear();
                    roleString.Append("Employee");
                    break;
                case Helpers.UserRole.Manager:
                    roleString.Clear();
                    roleString.Append("Manager");
                    break;
                default:
                    roleString.Clear();
                    roleString.Append("Employee");
                    break;
            }

            var pathDocument = new JsonPatchDocument();
            pathDocument.Replace("role", roleString.ToString());
            pathDocument.ApplyTo(userFromDb);
            await _context.SaveChangesAsync();
        }
        public override async Task Delete(int id)
        {
            var userFromDb = await _context.Users.AsQueryable()
                .FirstOrDefaultAsync(u => u.Id == id);
            if (userFromDb == null)
            {
                throw new BadHttpRequestException($"User with id:{id} doesn't exist");
            }

            var managers = _context.Users.AsQueryable().Where(x => x.Role == "Manager");
            if (managers.Count() == 1)
            {
                if (userFromDb.Role == "Manager")
                {
                    throw new BadHttpRequestException(
                        "You are the only manager and you can't delete yourself");
                }
            }

            _context.Remove(userFromDb);
            await _context.SaveChangesAsync();
        }
        public async Task<TokenModel> RefreshToken(TokenModel tokenModel)
        {
            if (tokenModel is null)
            {
                throw new BadHttpRequestException("Invalid client request");
            }

            var accessToken = tokenModel.AccessToken;
            var refreshToken = tokenModel.RefreshToken;
            var principal = GetPrincipalFromExpiredToken(accessToken);
            var userEmail = principal.Identity.Name;
            var userFromRepo = await _context.Users.AsQueryable()
                .FirstOrDefaultAsync(ue => ue.Email.ToLower() == userEmail.ToLower());
            if (userFromRepo == null || userFromRepo.RefreshToken != refreshToken ||
                userFromRepo.RefreshTokenExpireTime <= DateTime.Now)
            {
                throw new BadHttpRequestException("Invalid client request");
            }

            var newAccessToken = CreateAccessToken(userFromRepo);
            var newRefreshToken = GenerateRefreshToken();
            userFromRepo.RefreshToken = newRefreshToken;
            await _context.SaveChangesAsync();

            return new TokenModel
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
        public async Task Revoke(string email)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == email);
            if (user == null)
            {
                throw new BadHttpRequestException("User doesn't exist");
            }

            user.RefreshToken = string.Empty;
            await _context.SaveChangesAsync();
        }


        // Private methods
        private string CreateAccessToken(UserModel user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(claims: claims, expires: DateTime.Now.AddMinutes(5),
                signingCredentials: creds);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience =
                    false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value)),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512Signature,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
        private void CreatePasswordHash(string password, string passwordConfirmation, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password != passwordConfirmation)
            {
                throw new BadHttpRequestException("Password confirmation is not correct");
            }

            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
