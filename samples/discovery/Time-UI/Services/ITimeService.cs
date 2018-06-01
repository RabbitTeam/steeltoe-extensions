using System;
using System.Threading.Tasks;

namespace Time_UI.Services
{
    public interface ITimeService
    {
        Task<DateTime> GetNowAsync();
    }
}