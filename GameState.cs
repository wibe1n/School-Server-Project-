using System;
using System.Collections.Generic;

namespace game_project
{
    public class GameState{
        public Guid id {get;set;}
        public Guid player1JoinID;
        public Guid player2JoinID;
        public int turn {get; set;} 
        public int moveCounter{get; set;}
        public int lastMove{get;set;}
        public bool isCompleted{get;set;}
        public int winner{get; set;}
        public List<List<char>> board {get; set;}
    }
}