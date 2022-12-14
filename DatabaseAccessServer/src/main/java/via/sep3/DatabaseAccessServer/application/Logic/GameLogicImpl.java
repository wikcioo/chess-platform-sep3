package via.sep3.DatabaseAccessServer.application.Logic;

import org.springframework.stereotype.Component;
import via.sep3.DatabaseAccessServer.application.LogicInterfaces.GameLogic;
import via.sep3.DatabaseAccessServer.domain.DTOs.GameCreationDto;
import via.sep3.DatabaseAccessServer.domain.Game;
import via.sep3.DatabaseAccessServer.domain.User;
import via.sep3.DatabaseAccessServer.repository.GameRepository;
import via.sep3.DatabaseAccessServer.repository.UserRepository;

import javax.annotation.Resource;
import java.util.Map;
import java.util.Optional;

@Component
public class GameLogicImpl implements GameLogic {
    @Resource
    private final GameRepository gameRepository;

    @Resource
    private final UserRepository userRepository;

    public GameLogicImpl(GameRepository gameRepository, UserRepository userRepository) {
        this.gameRepository = gameRepository;
        this.userRepository = userRepository;
    }

    @Override
    public Game create(GameCreationDto dto) {
        User creator = userRepository.findByUsernameEquals(dto.getCreator()).orElseThrow(() -> new IllegalArgumentException("Creator does not exist."));
        User playerBlack = userRepository.findByUsernameEquals(dto.getPlayerBlack()).orElseThrow(() -> new IllegalArgumentException("Player Black does not exist."));
        User playerWhite = userRepository.findByUsernameEquals(dto.getPlayerWhite()).orElseThrow(() -> new IllegalArgumentException("Player White does not exist."));
        Game game = new Game(creator, playerWhite, playerBlack, dto.getGameType(), dto.getTimeControlDurationSeconds(), dto.getTimeControlIncrementSeconds(), dto.getGameOutcome());
        return gameRepository.save(game);
    }

    @Override
    public Iterable<Game> getAll(Map<String, String> allRequestParams) {
        Iterable<Game> games;
        if (!allRequestParams.containsKey("gameId")) {
            games = gameRepository.findAll();
        } else {
            games = gameRepository.findByCreatorEquals(allRequestParams.get("creator"));
        }
        return games;
    }

    @Override
    public Optional<Game> getByGameId(int gameId) {
        Optional<Game> game;
        game = gameRepository.findByGameId(gameId);
        return game;
    }
}
