package via.sep3.DatabaseAccessServer.controller;

import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;
import via.sep3.DatabaseAccessServer.domain.Game;
import via.sep3.DatabaseAccessServer.repository.GameRepository;

import javax.annotation.Resource;
import java.util.Map;

@RestController

public class GameController {
    @Resource
    private final GameRepository repository;

    public GameController(GameRepository repository) {
        this.repository = repository;
    }

    @GetMapping(path = "/games",
            produces = MediaType.APPLICATION_JSON_VALUE)
    public Iterable<Game> getAll(@RequestParam Map<String, String> allRequestParams) {
        Iterable<Game> games;

        if(!allRequestParams.containsKey("gameId")) {
            games = repository.findAll();
        }
        else {
            games = repository.findByCreatorIgnoreCase(allRequestParams.get("creator"));
        }
        return games;
    }


}
