package via.sep3.DatabaseAccessServer.application.LogicInterfaces;

import org.springframework.stereotype.Component;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestParam;
import via.sep3.DatabaseAccessServer.domain.DTOs.GameCreationDto;
import via.sep3.DatabaseAccessServer.domain.Game;

import java.util.Map;
import java.util.Optional;

public interface GameLogic {

    Game create(@RequestBody GameCreationDto dto);

    Iterable<Game> getAll(@RequestParam Map<String, String> allRequestParams);

    Optional<Game> getByGameId(@PathVariable("gameId") int gameId);
}
