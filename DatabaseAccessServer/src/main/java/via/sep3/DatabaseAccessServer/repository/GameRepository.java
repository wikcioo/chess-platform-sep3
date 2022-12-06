package via.sep3.DatabaseAccessServer.repository;

import org.springframework.data.repository.CrudRepository;
import org.springframework.data.repository.query.Param;
import via.sep3.DatabaseAccessServer.domain.Game;

import java.util.Optional;

public interface GameRepository extends CrudRepository<Game, String> {
Iterable<Game> findByCreatorIgnoreCase(@Param("creator") String creator);
Optional<Game> findByGameId(@Param("gameId") int gameId);
}
