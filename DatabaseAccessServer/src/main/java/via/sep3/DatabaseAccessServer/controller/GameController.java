package via.sep3.DatabaseAccessServer.controller;

import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.server.ResponseStatusException;
import via.sep3.DatabaseAccessServer.application.LogicInterfaces.GameLogic;
import via.sep3.DatabaseAccessServer.domain.DTOs.GameCreationDto;
import via.sep3.DatabaseAccessServer.domain.Game;

import java.util.Map;
import java.util.Optional;

@RestController
public class GameController {

    private final GameLogic gameLogic;


    public GameController(GameLogic gameLogic) {
        this.gameLogic = gameLogic;
    }


    @PostMapping(path = "/games",
            produces = MediaType.APPLICATION_JSON_VALUE)
    public Game create(@RequestBody GameCreationDto dto) {
        try {
            return gameLogic.create(dto);
        } catch (IllegalArgumentException e) {
            throw new ResponseStatusException(HttpStatus.BAD_REQUEST, e.getMessage());
        }
    }

    @GetMapping(path = "/games",
            produces = MediaType.APPLICATION_JSON_VALUE)
    public Iterable<Game> getAll(@RequestParam Map<String, String> allRequestParams) {
        return gameLogic.getAll(allRequestParams);
    }

    @GetMapping(path = "/games/{gameId}",
            produces = MediaType.APPLICATION_JSON_VALUE)
    public Optional<Game> getByGameId(@PathVariable("gameId") int gameId) {
        return gameLogic.getByGameId(gameId);
    }


}
