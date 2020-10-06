using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;

namespace game_project
{
    public class MongoRepo
    {
        private string breakTag = Environment.NewLine;
        private string address = "mongodb://localhost:27017";
        private IMongoCollection<GameState> Collection;
        private IMongoCollection<BsonDocument> DocumentsCollection;
        public MongoRepo(){
            var Client = new MongoClient(address);
            IMongoDatabase Database = Client.GetDatabase("Connect4In10");
            Collection = Database.GetCollection<GameState>("Games");
            DocumentsCollection = Database.GetCollection<BsonDocument>("Games");
        }

        public async Task<string> GetOngoingGames(){
            var result = Collection.Aggregate()
                .Project(r=> new {ID = r.id, Finished = r.isCompleted, 
                    MovesGiven = r.moveCounter, Turn = r.turn, P1 = r.player1JoinID, P2 = r.player2JoinID})
                .Match(r=> r.Finished == false) //&& (r.P1 == Guid.Empty || r.P2 == Guid.Empty)
                .SortBy(r=> r.MovesGiven)
                .Limit(5);
            
            var resList = await result.ToListAsync();
            string returnee = "";
            int i = 1;
            foreach(var member in resList){
                int playerCount = 0;
                if (member.P1 != Guid.Empty){
                    playerCount++;
                }
                if (member.P2 != Guid.Empty){
                    playerCount++;
                }
                returnee += i+" |"+member.ID+"| Players "+playerCount+"/"+2+ 
                    " | Moves given "+member.MovesGiven+"."+breakTag;
                i++;
            }
            if (returnee == ""){
                return "No games found."+breakTag;
            }
            return returnee;
        }

        public async Task<Guid> AddGame(GameState game){
            await Collection.InsertOneAsync(game);
            return game.id;
        }

        public async Task JoinAGame(Guid id, Guid playerID){
            GameState state = await GetBoard(id);
            if (state == null){
                throw new InvalidGameIDException("Game you tried to join does not exist.");
            }
            string updateDef = "";
            if (state.player1JoinID != Guid.Empty && state.player1JoinID != playerID){
                if (state.player2JoinID != Guid.Empty && state.player2JoinID != playerID){
                    throw new InvalidMoveException("Game is full.");
                } else {
                    updateDef = "player2JoinID";
                }
            } else {
                updateDef = "player1JoinID";
            }
            await Collection.UpdateOneAsync(Builders<GameState>.Filter.Eq("_id", state.id),
                Builders<GameState>.Update.Set(updateDef, playerID));
        }

        public async Task<GameState> GetBoard(Guid id){
            var filter = Builders<GameState>.Filter.Eq("_id", id);
            var result = Collection.Find(filter);
            long count = await result.CountDocumentsAsync();
            if (count == 0){
                throw new InvalidGameIDException("No board found with ID: "+id);
            }
            return result.FirstAsync().Result;
        }

        public async Task<GameState> GetBoardByPlayer(Guid playerID){
            return await GetBoardByPlayerID(playerID);
        }

        public async Task<GameState> MakeAMove(GameState state, int move, int nextTurn){
            // move is here in 1-9 format, char has been already inserted to state
            
            UpdateDefinition<GameState> upda = Builders<GameState>.Update
                .Set("board", state.board)
                .Inc("moveCounter", 1)
                .Set("lastMove", move)
                .Set("turn", nextTurn);
            var updateResult = await Collection.FindOneAndUpdateAsync(Builders<GameState>.Filter.Eq("_id", state.id),
                upda);
            if (updateResult != null){
                return await GetBoard(updateResult.id);
            }
            throw new InvalidMoveException("Something went wrong, update returned null."); 
        }

        public async Task<bool> DeleteAll(){
            await Collection.DeleteManyAsync(a=>true);
            return true;
        }

        public async Task<bool> SetState(GameState state){
            var filter = Builders<GameState>.Filter.Eq("_id", state.id);
            var result = await Collection.FindOneAndReplaceAsync(filter,state);
            return true;
        }

        public async Task<GameState> GetBoardByPlayerID(Guid id){
            var filter1 = Builders<GameState>.Filter.Eq("player1JoinID", id);
            var filter2 = Builders<GameState>.Filter.Eq("player2JoinID", id);
            var res1 = Collection.Find(filter1);
            var res2 = Collection.Find(filter2);
            var count1 = await res1.CountDocumentsAsync();
            var count2 = await res2.CountDocumentsAsync();
            if (count1 > 0){
                return await res1.FirstAsync();
            }
            if (count2 > 0){
                return await res2.FirstAsync();
            }
            throw new InvalidGameIDException("PlayerID didn't match any games");
        }
    }
}