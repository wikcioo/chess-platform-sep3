package via.sep3.DatabaseAccessServer.controller;

import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.stereotype.Component;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.server.ResponseStatusException;
import via.sep3.DatabaseAccessServer.application.LogicInterfaces.UserLogic;
import via.sep3.DatabaseAccessServer.domain.DTOs.UserLoginDto;
import via.sep3.DatabaseAccessServer.domain.User;

import java.util.Map;
import java.util.Optional;

@RestController
@Component
public class UserController {

    private final UserLogic userLogic;

    public UserController(UserLogic userLogic) {
        this.userLogic = userLogic;
    }

    @PostMapping(path = "/users",
            produces = MediaType.APPLICATION_JSON_VALUE)
    public User create(@RequestBody User user) {
        try {
            return userLogic.create(user);
        } catch (IllegalArgumentException e) {
            throw new ResponseStatusException(HttpStatus.BAD_REQUEST, e.getMessage());
        }
    }

    @PostMapping(path = "/login",
            produces = MediaType.APPLICATION_JSON_VALUE)
    public User login(@RequestBody UserLoginDto user) {
        try {
            return userLogic.login(user);
        } catch (IllegalArgumentException e) {
            throw new ResponseStatusException(HttpStatus.BAD_REQUEST, e.getMessage());
        }
    }

    @GetMapping(path = "/users",
            produces = MediaType.APPLICATION_JSON_VALUE)
    public Iterable<User> getAll(@RequestParam Map<String, String> allRequestParams) {
        return userLogic.getAll(allRequestParams);
    }

    @GetMapping(path = "/users/{username}",
            produces = MediaType.APPLICATION_JSON_VALUE)
    public Optional<User> getByUsername(@PathVariable("username") String username) {
        return userLogic.getByUsername(username);
    }
}
