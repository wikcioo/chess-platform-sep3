package via.sep3.DatabaseAccessServer.repository;

import org.springframework.data.repository.CrudRepository;
import org.springframework.data.repository.query.Param;
import org.springframework.lang.Nullable;
import org.springframework.stereotype.Repository;
import via.sep3.DatabaseAccessServer.domain.User;

import javax.annotation.Resource;
import java.util.Optional;

@Repository
@Resource
public interface UserRepository extends CrudRepository<User, String> {
    Optional<User> findByEmail(@Param("Email") String email);
}
