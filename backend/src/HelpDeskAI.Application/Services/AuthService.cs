using AutoMapper;
using HelpDeskAI.Application.Common;
using HelpDeskAI.Application.Contracts;
using HelpDeskAI.Application.DTOs;
using HelpDeskAI.Domain.Entities;

namespace HelpDeskAI.Application.Services;

public sealed class AuthService(IUserRepository users, IUnitOfWork unitOfWork, IPasswordHasher passwords, ITokenService tokens, IMapper mapper)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.GetByEmailAsync(email, cancellationToken) is not null)
            throw new ConflictException("An account with this email already exists.");

        var user = new User { Name = request.Name.Trim(), Email = email, PasswordHash = passwords.Hash(request.Password) };
        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var dto = mapper.Map<UserDto>(user);
        return new AuthResponse(tokens.CreateToken(user.Id, user.Email, user.Name, user.Role.ToString()), dto);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !passwords.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");
        var dto = mapper.Map<UserDto>(user);
        return new AuthResponse(tokens.CreateToken(user.Id, user.Email, user.Name, user.Role.ToString()), dto);
    }
}
