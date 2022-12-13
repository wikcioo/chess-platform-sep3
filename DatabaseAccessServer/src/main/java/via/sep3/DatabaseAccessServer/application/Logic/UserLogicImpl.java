package via.sep3.DatabaseAccessServer.application.Logic;

import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Component;
import org.springframework.web.server.ResponseStatusException;
import via.sep3.DatabaseAccessServer.application.LogicInterfaces.UserLogic;
import via.sep3.DatabaseAccessServer.domain.DTOs.UserLoginDto;
import via.sep3.DatabaseAccessServer.domain.User;
import via.sep3.DatabaseAccessServer.repository.UserRepository;

import javax.annotation.Resource;
import java.util.Map;
import java.util.Optional;

@Component
public class UserLogicImpl implements UserLogic {

    @Resource
    private final UserRepository userRepository;

    public UserLogicImpl(UserRepository userRepository) {
        this.userRepository = userRepository;
    }

    @Override
    public User create(User user) {
        if (user.getUsername().toLowerCase().contains("stockfishai")) {
            throw new IllegalArgumentException("Usernames starting with stockfishai are not allowed");
        }
        if (userRepository.findByEmailIgnoreCase(user.getEmail()).isPresent()) {
            throw new IllegalArgumentException("This email is already in use");
        }
        if (userRepository.findByUsernameEquals(user.getUsername()).isPresent()) {
            throw new IllegalArgumentException("This username is already in use");
        }
        return userRepository.save(user);
    }

    @Override
    public User login(UserLoginDto user) {
        User existing = userRepository.findByEmailIgnoreCase(user.getEmail()).orElseThrow(() -> new IllegalArgumentException("This user does not exist"));
        if (existing.getPassword().equals(user.getPassword())) {
            return existing;
        } else {
            throw new IllegalArgumentException("Incorrect credentials");
        }
    }

    @Override
    public Iterable<User> getAll(Map<String, String> allRequestParams) {
        Iterable<User> users;

        if (!allRequestParams.containsKey("username")) {
            users = userRepository.findAll();
        } else {
            users = userRepository.findByUsernameContaining(allRequestParams.get("username"));
        }
        return users;
    }

    @Override
    public Optional<User> getByUsername(String username) {
        Optional<User> user;
        user = userRepository.findByUsernameEquals(username);
        return user;
    }
}
