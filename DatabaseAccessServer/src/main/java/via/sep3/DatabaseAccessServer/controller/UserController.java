package via.sep3.DatabaseAccessServer.controller;

import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.server.ResponseStatusException;
import via.sep3.DatabaseAccessServer.domain.User;
import via.sep3.DatabaseAccessServer.repository.UserRepository;

import javax.annotation.Resource;
import java.util.List;

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
        return repository.save(user);
    }

    @PostMapping(path = "/login",
            produces = MediaType.APPLICATION_JSON_VALUE)
    public Boolean Login(@RequestBody User user) throws Exception {
        User existing = repository.findByEmail(user.getEmail()).orElseThrow(() -> new ResponseStatusException(HttpStatus.NOT_FOUND,"This user does not exist"));
        return existing.getPassword().equals(user.getPassword());
    }

    @GetMapping(path = "/users",
            produces = MediaType.APPLICATION_JSON_VALUE)
    public Iterable<User> GetAll() {
        return repository.findAll();
    }

}
