using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace game_project
{
    public class GameProcessor
    {
        private string breakTag = Environment.NewLine;
        MongoRepo repository;
        public GameProcessor(MongoRepo mongoRep){
            this.repository = mongoRep;
        }

        public Task<string> GetOngoingGames(){
            return repository.GetOngoingGames();
        }

        public async Task<string> GetBoardInfo(Guid id){
            GameState state = await repository.GetBoard(id);
            return GetVisualBoard(state, null);
        }

        public async Task<string> GetBoardInfoByPlayer(Guid playerID){
            GameState state = await repository.GetBoardByPlayer(playerID);
            return GetVisualBoard(state, playerID);
        }

        public Task<Guid> CreateNewGame(){
            GameState state = new GameState();
            state.id = Guid.NewGuid();
            state.isCompleted = false;
            state.lastMove = -1;
            state.moveCounter = 0;
            List<char> tempList = new List<char>();
            for (int j = 0; j<9;j++){
                tempList.Add(' ');
            }
            state.board = new List<List<char>>();
            for(int i = 0; i< 9;i++){
                state.board.Add(tempList);
            }
            return repository.AddGame(state);
        }

        public async Task<Guid> JoinAGame(Guid id){
            Guid playerID = Guid.NewGuid();
            await repository.JoinAGame(id, playerID);
            return playerID;
        }

        public async Task<string> Play(Guid playerID, int? move){
            if (move == null){
                throw new InvalidMoveException("No move given.");
                // Should not happen
            }
            if (move < 1 || move > 9){
                throw new InvalidMoveException("Move out of range [1-9]");
            }
            int movennIndex = (int)(move-1);
            char ch;
            int player;
            GameState state = await repository.GetBoardByPlayerID(playerID);
            if (state.player1JoinID == playerID){
                player = 0;
            } else if (state.player2JoinID == playerID){
                player = 1;
            } else {
                throw new InvalidGameIDException("Should not happen error.");
            }
            if (state == null){
                throw new InvalidGameIDException("playerID didn't match any games.");
            }
            if (player == 1){
                ch = 'X';
            } else {
                ch = 'O';
            }
            if (state.isCompleted){
                throw new InvalidMoveException("Tried to insert to a finished game.");
            }
            if (state.turn != player){
                throw new InvalidMoveException("Wrong player (player"+player+") attempted to make a move.");
            }
            int prevIndex = -1;
            for(int i = 8; i> -1; i--){
                if (state.board[i][movennIndex] != ' '){
                    if (prevIndex == -1){
                        throw new InvalidMoveException("Tried to insert to full row.");
                    }
                    state.board[prevIndex][movennIndex] = ch;
                    break;
                }
                prevIndex = i;
            }
            if (prevIndex == 0){
                state.board[prevIndex][movennIndex]=ch;
            }
            int nextTurn = -1;
            if (state.turn == 0){
                nextTurn = 1;
            }else{
                nextTurn = 0;
            }
            GameState stateNew = await repository.MakeAMove(state, movennIndex, nextTurn);
            await CheckEndState(stateNew);
            return GetVisualBoard(stateNew, playerID);
        }

        public Task<bool> DeleteAll(){
            return repository.DeleteAll();
        }

        private async Task CheckEndState(GameState state){
            if (state.turn == 0){
                if(CheckEndState(state, 'X')){
                    state.isCompleted = true;
                    state.winner = 1;
                    await repository.SetState(state);
                }
            } else {
                if (CheckEndState(state, 'O')){
                    state.isCompleted = true;
                    state.winner = 0;
                    await repository.SetState(state);
                }
            }
            if (CheckDraw(state)){
                state.isCompleted = true;
                state.winner = -1;
                await repository.SetState(state);
            }
        }

        private bool CheckDraw(GameState state){
            foreach(List<char> sub in state.board){
                foreach(char ch in sub){
                    if (ch == ' '){
                        return false;
                    }
                }
            }
            return true;
        }
        private bool CheckEndState(GameState state, char ch){
            // horizontalCheck 
            int width = 9;
            int height = 9;
            for (int j = 0; j<height-3 ; j++ ){
                for (int i = 0; i<width; i++){
                    if (state.board[i][j] == ch && state.board[i][j+1] == ch && state.board[i][j+2] == ch && state.board[i][j+3] == ch){
                        return true;
                    }           
                }
            }
            // verticalCheck
            for (int i = 0; i<width-3 ; i++ ){
                for (int j = 0; j<height; j++){
                    if (state.board[i][j] == ch && state.board[i+1][j] == ch && state.board[i+2][j] == ch && state.board[i+3][j] == ch){
                        return true;
                    }           
                }
            }
            // ascendingDiagonalCheck 
            for (int i=3; i<width; i++){
                for (int j=0; j<height-3; j++){
                    if (state.board[i][j] == ch && state.board[i-1][j+1] == ch && state.board[i-2][j+2] == ch && state.board[i-3][j+3] == ch)
                        return true;
                }
            }
            // descendingDiagonalCheck
            for (int i=3; i<width; i++){
                for (int j=3; j<height; j++){
                    if (state.board[i][j] == ch && state.board[i-1][j-1] == ch && state.board[i-2][j-2] == ch && state.board[i-3][j-3] == ch)
                        return true;
                }
            }
            return false;
        }

        private string GetVisualBoard(GameState state, Guid? playerID){
            int player = -1;
            if (playerID != null){
                if (state.player1JoinID == playerID){
                    player = 0;
                } else if (state.player2JoinID == playerID) {
                    player = 1;
                }
            }
            string returnee = "";
            if (!state.isCompleted){
                if (player == -1){
                    returnee += state.turn+": It's player "+state.turn+"'s turn.";
                }else{
                    if (player == state.turn){
                        returnee += "TRUE: It is your turn";
                    } else if (player != state.turn){
                        returnee += "FALSE: It is not your turn.";
                    } else {
                        returnee += "ERROR: player was something unexpected.";
                        //Should never happen.
                    }
                }
            } else {
                returnee += "OVER: Game completed!"+breakTag;
                if (player == -1){
                    switch(state.winner){
                        case 0:
                            returnee += "Player 0 is winner!";
                            break;
                        case 1:
                            returnee += "Player 1 is winner!";
                            break;
                        case -1:
                            returnee += "It's a draw.";
                            break;
                        default:
                            returnee += "Error: Illegal winnerstatus.";
                            break;
                    }
                } else {
                    if (player == state.winner){
                        returnee += "You are the winner!";
                    } else if (state.winner == -1){
                        returnee += "It's a draw";
                    } else {
                        returnee += "You lost.";
                    }
                }
            }
            returnee += breakTag;
            for (int i = 8; i>-1; i--){
                returnee += " "+(i+1)+" ";
                for (int j = 0;j<9;j++){
                    returnee += " "+state.board[i][j]+" ";
                }
                returnee += breakTag;
            }
            returnee += "   ";
            for(int i = 0;i<9;i++){
                returnee += " "+(i+1)+" ";
            }
            returnee += breakTag;
            return returnee;
        }
    }

    
}