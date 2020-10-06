using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace game_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [InvalidGameIDExceptionFilter][InvalidMoveExceptionFilter]
    public class GameController : ControllerBase
    {  
        private string breakTag = Environment.NewLine;
        private GameProcessor processor;
        public GameController(GameProcessor processor){
            this.processor = processor;
        }

        [HttpGet("Info")]
        public Task<string> GetInfo(){
            return Task.FromResult(
                "This is a game of connect 4, in 9x9 grid."+breakTag
             + "You can create, join or spectate games."+breakTag
             + "While playing or spectating game(s), you can press ESC to return to menu."+breakTag
             + "Player 0 is 'O' and player 1 is 'X' on the board."+breakTag
             + "GameID is Guid that is created for each game, GameNumber is number next to GameID in Find command."+breakTag
             + "PlayerID is Guid you get from joining. Move is given as int, representing the insert column [1-9]"+breakTag
             + "After inserting a move and returning the result state, the game will wait until opponent makes a move."+breakTag
             + "Have fun."+breakTag
             );
        }

        [HttpGet]
        public Task<string> GetOngoingGamesData(){
            return processor.GetOngoingGames();
        }

        [HttpGet("New")]
        public Task<Guid> CreateNewGame(){
            return processor.CreateNewGame();
        }

        [HttpGet("{id:guid}")]
        public Task<string> GetAGame(Guid id){
            return processor.GetBoardInfo(id);
        }

        [HttpGet("Join/{id:guid}")]
        public async Task<Guid> JoinAGame(Guid id){
            Guid playerGuid = await processor.JoinAGame(id);
            return playerGuid;
        }

        [HttpGet("Play/{playerID:guid}")]
        public Task<string> Play(Guid playerID, [FromQuery] int? move){
            if (move == null){
                return processor.GetBoardInfoByPlayer(playerID);
            }
            return processor.Play(playerID, move);
        }

        [HttpDelete]
        public Task<bool> DeleteAllGames(){
            return processor.DeleteAll();
        }
    }
}