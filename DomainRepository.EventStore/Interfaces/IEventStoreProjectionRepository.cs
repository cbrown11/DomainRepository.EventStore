using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI.Projections;

namespace DomainRepository.EventStore.Interfaces
{
    public interface IEventStoreProjectionRepository
    {
        Task<List<ProjectionDetails>> ListProjectionsAsync();
        Task CreateContinuousProjections(string name, string projection);
        Task UpdateProjections(string name, string projection);
        Task DeleteProjection(string name);
        Task<string> GetState(string projectionName);
        T GetState<T>(string projectionName);
        Task<string> GetResult(string projectionName);

        bool ProjectionExists(string name);
    }
}