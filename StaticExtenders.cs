using Microsoft.Extensions.DependencyInjection;

namespace game_project
{
    public static class StaticExtenders
    {
        public static IServiceCollection UseRepository(this IServiceCollection services){
            return services.AddSingleton<MongoRepo>();
        }

        public static IServiceCollection UseGameProcessor(this IServiceCollection services){
            return services.AddSingleton<GameProcessor>();
        }
    }
}