package via.sep3.DatabaseAccessServer.domain.DTOs;

import via.sep3.DatabaseAccessServer.domain.User;
import via.sep3.DatabaseAccessServer.domain.enums.GameType;

import javax.persistence.Enumerated;
import javax.persistence.Id;
import javax.persistence.JoinColumn;
import javax.persistence.ManyToOne;

public class GameCreationDto {


    private String creator;
    private String playerWhite;

    private String playerBlack;

    private GameType gameType;
    private int timeControlDurationSeconds;
    private int timeControlIncrementSeconds;

    public GameCreationDto(String creator, String playerWhite, String playerBlack, GameType gameType, int timeControlDurationSeconds, int timeControlIncrementSeconds) {
        this.creator = creator;
        this.playerWhite = playerWhite;
        this.playerBlack = playerBlack;
        this.gameType = gameType;
        this.timeControlDurationSeconds = timeControlDurationSeconds;
        this.timeControlIncrementSeconds = timeControlIncrementSeconds;
    }


    public String getCreator() {
        return creator;
    }

    public void setCreator(String creator) {
        this.creator = creator;
    }

    public String getPlayerWhite() {
        return playerWhite;
    }

    public void setPlayerWhite(String playerWhite) {
        this.playerWhite = playerWhite;
    }

    public String getPlayerBlack() {
        return playerBlack;
    }

    public void setPlayerBlack(String playerBlack) {
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

    public void setTimeControlDurationSeconds(int timeControlDurationSeconds) {
        this.timeControlDurationSeconds = timeControlDurationSeconds;
    }

    public int getTimeControlIncrementSeconds() {
        return timeControlIncrementSeconds;
    }

    public void setTimeControlIncrementSeconds(int timeControlIncrementSeconds) {
        this.timeControlIncrementSeconds = timeControlIncrementSeconds;
    }
}
