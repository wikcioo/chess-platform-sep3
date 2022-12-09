package via.sep3.DatabaseAccessServer.domain;

import via.sep3.DatabaseAccessServer.domain.enums.GameOutcome;
import via.sep3.DatabaseAccessServer.domain.enums.GameType;

import javax.persistence.*;

@Entity
@Table(name = "games")
public class Game {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int gameId;
    @ManyToOne
    @JoinColumn(name="creator", referencedColumnName = "email")
    private User creator;
    @ManyToOne
    @JoinColumn(name="playerWhite", referencedColumnName = "email")
    private User playerWhite;
    @ManyToOne
    @JoinColumn(name="playerBlack", referencedColumnName = "email")
    private User playerBlack;
    @Enumerated
    private GameType gameType;
    private int timeControlDurationSeconds;
    private int timeControlIncrementSeconds;

    private GameOutcome gameOutcome;


    public Game(){};


    public Game(User creator,  GameType gameType, User playerWhite, User playerBlack, int timeControlDurationSeconds, int timeControlIncrementSeconds, GameOutcome gameOutcome){
        this.creator = creator;
        this.gameType = gameType;
        this.playerWhite = playerWhite;
        this.playerBlack = playerBlack;
        this.timeControlDurationSeconds = timeControlDurationSeconds;
        this.timeControlIncrementSeconds = timeControlIncrementSeconds;
        this.gameOutcome = gameOutcome;

    }

    public void setTimeControlDurationSeconds(int timeControlDurationSeconds) {
        this.timeControlDurationSeconds = timeControlDurationSeconds;
    }

    public void setTimeControlIncrementSeconds(int timeControlIncrementSeconds) {
        this.timeControlIncrementSeconds = timeControlIncrementSeconds;
    }

    public GameOutcome getGameOutcome() {
        return gameOutcome;
    }

    public void setGameOutcome(GameOutcome gameOutcome) {
        this.gameOutcome = gameOutcome;
    }

    public int getGameId() {
        return gameId;
    }

    public void setGameId(int gameId) {
        this.gameId = gameId;
    }

    public User getCreator() {
        return creator;
    }

    public void setCreator(User creator) {
        this.creator = creator;
    }

    public User getPlayerWhite() {
        return playerWhite;
    }

    public void setPlayerWhite(User playerWhite) {
        this.playerWhite = playerWhite;
    }

    public User getPlayerBlack() {
        return playerBlack;
    }

    public void setPlayerBlack(User playerBlack) {
        this.playerBlack = playerBlack;
    }

    public GameType getGameType() {
        return gameType;
    }

    public void setGameType(GameType gameType) {
        this.gameType = gameType;
    }

    public int getTimeControlDurationSeconds() {
        return timeControlDurationSeconds;
    }

    public int getTimeControlIncrementSeconds() {
        return timeControlIncrementSeconds;
    }
}
