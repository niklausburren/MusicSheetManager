using System.Threading;
using System.Threading.Tasks;
using MusicSheetManager.Models;

namespace MusicSheetManager.Services;

public interface IMusicSheetDistributionService
{
    #region Public Methods

    Task DistributeAsync(IDistributionReporter reporter, CancellationToken cancellationToken = default);

    void ExportSheetDistribution(Playlist playlistService);

    #endregion
}