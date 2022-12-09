package via.sep3.DatabaseAccessServer.repository;

import org.springframework.data.repository.CrudRepository;
import org.springframework.data.repository.query.Param;
import org.springframework.stereotype.Repository;
import via.sep3.DatabaseAccessServer.domain.Game;
import via.sep3.DatabaseAccessServer.domain.User;

import javax.annotation.Resource;
import java.util.Optional;
@Repository
@Resource
public interface GameRepository extends CrudRepository<Game, String> {
    Optional<Game> findByGameId(@Param("gameId") int gameId);
    Iterable<Game> findByCreatorEquals(@Param("creator") String creator);


}
