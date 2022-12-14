package via.sep3.DatabaseAccessServer;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.orm.jpa.DataJpaTest;
import via.sep3.DatabaseAccessServer.application.Logic.UserLogicImpl;
import via.sep3.DatabaseAccessServer.application.LogicInterfaces.UserLogic;
import via.sep3.DatabaseAccessServer.domain.DTOs.UserLoginDto;
import via.sep3.DatabaseAccessServer.domain.User;
import via.sep3.DatabaseAccessServer.repository.UserRepository;


import java.util.Optional;

import static org.junit.jupiter.api.Assertions.*;

@DataJpaTest
public class UserLogicUnitTests {

    @Autowired
    private UserRepository userRepository;
    private UserLogic userLogic;
    User user;

    @BeforeEach
    void init() {
        userLogic = new UserLogicImpl(userRepository);
        user = new User("email", "username", "password", "admin");
        userLogic.create(user);
    }

    //Create
    @Test
    void creatingUserWithStockfishaiThrowsIllegalArgumentException() {
        user = new User("stockfishai", "stockfishai", "password", "admin");
        assertThrows(IllegalArgumentException.class, () -> userLogic.create(user));
    }

    @Test
    void creatingUserWithAnAlreadyInUseEmailThrowsIllegalArgumentExceptions() {
        assertThrows(IllegalArgumentException.class, () -> userLogic.create(user));
    }

    @Test
    void creatingUserWithAnAlreadyInUseUsernameThrowsIllegalArgumentExceptions() {
        user.setEmail("differentEmail");
        assertThrows(IllegalArgumentException.class, () -> userLogic.create(user));
    }

    @Test
    void creatingUserWithValidFieldsReturnsUser() {
        user = new User("newEmail", "newUsername", "password", "admin");
        User createdUser = userLogic.create(user);
        assertEquals(user, createdUser);
    }

    //Login
    @Test
    void loggingInWithIncorrectCredentialsThrowsIllegalArgumentExceptions() {
        UserLoginDto dto = new UserLoginDto("email", "wrongPassword");
        assertThrows(IllegalArgumentException.class, () -> userLogic.login(dto));
    }

    @Test
    void loggingInWithNonExistingUserThrowsIllegalArgumentExceptions() {
        UserLoginDto dto = new UserLoginDto("nonExistentEmail", "password");
        assertThrows(IllegalArgumentException.class, () -> userLogic.login(dto));
    }

    @Test
    void loggingInWithValidCredentialsReturnsUser() {
        UserLoginDto dto = new UserLoginDto("email", "password");
        User createdUser = userLogic.login(dto);
        assertEquals(user, createdUser);
    }

    //Get by username
    @Test
    void getByUsernameReturnCorrectUserWhenUserExists() {
        Optional<User> found = userLogic.getByUsername("username");
        assertTrue(found.isPresent());
    }

    @Test
    void getByUsernameReturnsEmptyWhenUserDoesNotExist() {
        Optional<User> found = userLogic.getByUsername("wrongUsername");
        assertFalse(found.isPresent());
    }
}
