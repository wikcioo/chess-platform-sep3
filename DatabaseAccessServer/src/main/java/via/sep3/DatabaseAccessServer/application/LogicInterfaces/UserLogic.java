package via.sep3.DatabaseAccessServer.application.LogicInterfaces;

import org.springframework.stereotype.Component;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestParam;
import via.sep3.DatabaseAccessServer.domain.DTOs.UserLoginDto;
import via.sep3.DatabaseAccessServer.domain.User;

import java.util.Map;
import java.util.Optional;

public interface UserLogic {

    User create(@RequestBody User user);

    User login(@RequestBody UserLoginDto user);

    Iterable<User> getAll(@RequestParam Map<String, String> allRequestParams);

    Optional<User> getByUsername(@PathVariable("username") String username);
}
