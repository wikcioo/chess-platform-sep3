package via.sep3.DatabaseAccessServer.controller;

import org.springframework.web.bind.annotation.RestController;
import via.sep3.DatabaseAccessServer.domain.User;
import via.sep3.DatabaseAccessServer.repository.UserRepository;

import javax.annotation.Resource;

@RestController
public class UserController {
    @Resource
    private final UserRepository repository;

    public UserController(UserRepository repository) {
        this.repository = repository;
    }

}
