namespace DotNetAPI.Dtos;

public partial class UserForLoginConfirmationDto
{
    public byte[] PasswordHash {get; set;}
    public byte[] PasswordSalt {get; set;}

    private UserForLoginConfirmationDto()
    {
        PasswordHash ??= [];
        PasswordSalt ??= [];
    }
}