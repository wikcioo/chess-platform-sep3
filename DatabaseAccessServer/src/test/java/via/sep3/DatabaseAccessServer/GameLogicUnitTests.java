package via.sep3.DatabaseAccessServer;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.orm.jpa.DataJpaTest;
import via.sep3.DatabaseAccessServer.application.Logic.GameLogicImpl;
import via.sep3.DatabaseAccessServer.application.LogicInterfaces.GameLogic;
import via.sep3.DatabaseAccessServer.domain.DTOs.GameCreationDto;
import via.sep3.DatabaseAccessServer.domain.Game;
import via.sep3.DatabaseAccessServer.domain.User;
import via.sep3.DatabaseAccessServer.domain.enums.GameOutcome;
import via.sep3.DatabaseAccessServer.domain.enums.GameType;
import via.sep3.DatabaseAccessServer.repository.GameRepository;
import via.sep3.DatabaseAccessServer.repository.UserRepository;

import java.util.Optional;

import static org.junit.jupiter.api.Assertions.*;
import static org.junit.jupiter.api.Assertions.assertFalse;

@DataJpaTest
public class GameLogicUnitTests {


    @Autowired
    private UserRepository userRepository;
    @Autowired
    private GameRepository gameRepository;

    private GameLogic gameLogic;
    private GameCreationDto gameCreationDto;

    @BeforeEach
    void init() {
        gameLogic = new GameLogicImpl(gameRepository, userRepository);
        User playerOne = new User("1", "1", "1", "1");
        User playerTwo = new User("2", "2", "2", "2");
        userRepository.save(playerOne);
        userRepository.save(playerTwo);
        Game game = new Game(playerOne, playerOne, playerTwo, GameType.RANDOM, 60, 5, GameOutcome.DRAW);
        gameCreationDto = new GameCreationDto(playerOne.getEmail(), playerOne.getEmail(), playerTwo.getEmail(), GameType.RANDOM, 60, 5, GameOutcome.DRAW);
    }

    //Create
    @Test
    void creatingGameWithNonExistentCreatorThrowIllegalArgumentException() {
        gameCreationDto.setCreator("nonExistent");
        assertThrows(IllegalArgumentException.class, () -> gameLogic.create(gameCreationDto));
    }


    @Test
    void creatingGameWithNonExistentPlayerWhiteThrowIllegalArgumentException() {
        gameCreationDto.setPlayerWhite("nonExistent");
        assertThrows(IllegalArgumentException.class, () -> gameLogic.create(gameCreationDto));
    }

    @Test
    void creatingGameWithNonExistentPlayerBlackThrowIllegalArgumentException() {
        gameCreationDto.setPlayerBlack("nonExistent");
        assertThrows(IllegalArgumentException.class, () -> gameLogic.create(gameCreationDto));
    }

    //Get by game id
    @Test
    void getByIdReturnGameWhenGameExists() {
        Game game = gameLogic.create(gameCreationDto);
        Optional<Game> found = gameLogic.getByGameId(game.getGameId());
        assertTrue(found.isPresent());
    }

    @Test
    void getByIdReturnEmptyWhenGameDoesNotExists() {
        gameLogic.create(gameCreationDto);
        Optional<Game> found = gameLogic.getByGameId(50);
        assertFalse(found.isPresent());
    }

}
