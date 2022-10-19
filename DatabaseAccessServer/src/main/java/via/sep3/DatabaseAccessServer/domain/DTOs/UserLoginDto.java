package via.sep3.DatabaseAccessServer.domain.DTOs;

public class UserLoginDto {
    public String email;
    public String password;

    public UserLoginDto(String email, String password) {
        this.email = email;
        this.password = password;
    }

    public String getEmail() {
        return email;
    }

    public String getPassword() {
        return password;
    }
}
