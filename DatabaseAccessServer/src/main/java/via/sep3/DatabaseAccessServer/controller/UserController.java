package via.sep3.DatabaseAccessServer.controller;

import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.server.ResponseStatusException;
import via.sep3.DatabaseAccessServer.domain.DTOs.UserLoginDto;
import via.sep3.DatabaseAccessServer.domain.User;
import via.sep3.DatabaseAccessServer.repository.UserRepository;

import javax.annotation.Resource;
import java.util.Iterator;
import java.util.Map;
import java.util.Optional;

@RestController
public class UserController {
    @Resource
    private final UserRepository repository;

    public UserController(UserRepository repository) {
        this.repository = repository;
    }

    @PostMapping(path = "/users",
            produces = MediaType.APPLICATION_JSON_VALUE)
    public User Create(@RequestBody User user) {
        if (user.getUsername().toLowerCase().contains("stockfishai")) {
            throw new ResponseStatusException(HttpStatus.BAD_REQUEST, "Usernames starting with stockfishai are not allowed");
        }
        if (repository.findByEmailIgnoreCase(user.getEmail()).isPresent()) {
            throw new ResponseStatusException(HttpStatus.BAD_REQUEST, "This email is already in use");
        }
        if (repository.findByUsernameEquals(user.getUsername()).isPresent()) {
            throw new ResponseStatusException(HttpStatus.BAD_REQUEST, "This username is already in use");
        }
        return repository.save(user);
    }

    @PostMapping(path = "/login",
            produces = MediaType.APPLICATION_JSON_VALUE)
    public User Login(@RequestBody UserLoginDto user) {
        User existing = repository.findByEmailIgnoreCase(user.getEmail()).orElseThrow(() -> new ResponseStatusException(HttpStatus.BAD_REQUEST, "This user does not exist"));
        if (existing.getPassword().equals(user.getPassword())) {
            return existing;
        } else {
            throw new ResponseStatusException(HttpStatus.BAD_REQUEST, "Incorrect credentials");
        }
    }

    @GetMapping(path = "/users",
            produces = MediaType.APPLICATION_JSON_VALUE)
    public Iterable<User> GetAll(@RequestParam Map<String,String> allRequestParams) {
        Iterable<User> users;

        if(!allRequestParams.containsKey("username")) {
            users = repository.findAll();
        }
        else {
            users = repository.findByUsernameContaining(allRequestParams.get("username"));
        }
        return users;
    }

    @GetMapping(path = "/users/{username}",
            produces = MediaType.APPLICATION_JSON_VALUE)
        public Optional<User> GetByUsername(@PathVariable("username") String username) {
        Optional<User> user;
        user = repository.findByUsernameEquals(username);
        return user;
    }
}
