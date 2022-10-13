package via.sep3.DatabaseAccessServer.repository;

import org.springframework.data.repository.CrudRepository;
import org.springframework.stereotype.Repository;
import via.sep3.DatabaseAccessServer.domain.User;

import javax.annotation.Resource;

@Repository
@Resource
public interface UserRepository extends CrudRepository<User, String> {

}
