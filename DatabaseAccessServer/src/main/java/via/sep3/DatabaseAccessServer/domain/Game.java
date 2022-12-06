package via.sep3.DatabaseAccessServer.domain;

import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.ManyToOne;
import javax.persistence.Table;

@Entity
@Table(name = "games")
public class Game {
    @Id
    private int gameId;
@ManyToOne
    private User creator;
    @ManyToOne
    private User playerWhite;
    @ManyToOne
    private User playerBlack;
    private enum GameType{AI, FRIEND, RANDOM};
    private GameType gameType;
    private enum GameSide{WHITE, BLACK, RANDOM};
    private GameSide gameSide;
    private int timeControlDurationSeconds;
    private int timeControlIncrementSeconds;

    public Game(){};


    public Game(User creator,  GameType gameType, User playerWhite, User playerBlack, GameSide gameSide,  int timeControlDurationSeconds, int timeControlIncrementSeconds){
        this.creator = creator;
        this.gameType = gameType;
        this.playerWhite = playerWhite;
        this.playerBlack = playerBlack;
        this.timeControlDurationSeconds = timeControlDurationSeconds;
        this.timeControlIncrementSeconds = timeControlIncrementSeconds;
        this.gameSide = gameSide;
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

    public GameSide getGameSide() {
        return gameSide;
    }

    public void setGameSide(GameSide gameSide) {
        this.gameSide = gameSide;
    }

    public int getTimeControlDurationSeconds() {
        return timeControlDurationSeconds;
    }

    public int getTimeControlIncrementSeconds() {
        return timeControlIncrementSeconds;
    }
}
