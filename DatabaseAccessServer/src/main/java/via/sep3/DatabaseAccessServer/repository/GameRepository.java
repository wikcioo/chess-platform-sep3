package via.sep3.DatabaseAccessServer.repository;

import org.springframework.data.repository.CrudRepository;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;
import via.sep3.DatabaseAccessServer.domain.Game;
import via.sep3.DatabaseAccessServer.domain.User;

import java.util.Optional;

public interface GameRepository extends CrudRepository<Game, String> {
Optional<Game> findByCreatorIgnoreCase(@Param("creator") String creator);
    Optional<Game> findByGameId(@Param("gameId") int gameId);
}
