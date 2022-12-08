package via.sep3.DatabaseAccessServer.controller;

import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.server.ResponseStatusException;
import via.sep3.DatabaseAccessServer.domain.DTOs.GameCreationDto;
import via.sep3.DatabaseAccessServer.domain.Game;
import via.sep3.DatabaseAccessServer.domain.User;
import via.sep3.DatabaseAccessServer.domain.enums.GameType;
import via.sep3.DatabaseAccessServer.repository.GameRepository;
import via.sep3.DatabaseAccessServer.repository.UserRepository;

import javax.annotation.Resource;
import java.util.Map;
import java.util.Optional;

@RestController

public class GameController {
    @Resource
    private final GameRepository gameRepository;

    @Resource
    private final UserRepository userRepository;


    public GameController(GameRepository gameRepository, UserRepository userRepository) {
        this.gameRepository = gameRepository;
        this.userRepository = userRepository;
    }


    @PostMapping(path = "/games",
            produces = MediaType.APPLICATION_JSON_VALUE)
    public Game Create(@RequestBody GameCreationDto dto) {
        User creator = userRepository.findByUsernameEquals(dto.getCreator()).orElseThrow(() -> new ResponseStatusException(HttpStatus.BAD_REQUEST, "Creator does not exist."));
        User playerBlack = userRepository.findByUsernameEquals(dto.getPlayerBlack()).orElseThrow(() -> new ResponseStatusException(HttpStatus.BAD_REQUEST, "Player Black does not exist."));
        User playerWhite = userRepository.findByUsernameEquals(dto.getPlayerWhite()).orElseThrow(() -> new ResponseStatusException(HttpStatus.BAD_REQUEST, "Player White does not exist."));
        Game game = new Game(creator, dto.getGameType(), playerWhite, playerBlack, dto.getTimeControlDurationSeconds(), dto.getTimeControlIncrementSeconds(), dto.getGameOutcome());
        return gameRepository.save(game);
    }

    @GetMapping(path = "/games",
            produces = MediaType.APPLICATION_JSON_VALUE)
    public Iterable<Game> GetAll(@RequestParam Map<String, String> allRequestParams) {
        Iterable<Game> games;
        if (!allRequestParams.containsKey("gameId")) {
            games = gameRepository.findAll();
        } else {
            games = gameRepository.findByCreatorEquals(allRequestParams.get("creator"));
        }
        return games;
    }

    @GetMapping(path = "/games/{gameId}",
            produces = MediaType.APPLICATION_JSON_VALUE)
    public Optional<Game> GetByGameId(@PathVariable("gameId") int gameId) {
        Optional<Game> game;
        game = gameRepository.findByGameId(gameId);
        return game;
    }


}
