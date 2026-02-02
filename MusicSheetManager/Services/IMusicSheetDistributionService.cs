using System.Threading;
using System.Threading.Tasks;

namespace MusicSheetManager.Services;

public interface IMusicSheetDistributionService
{
    #region Public Methods

    Task DistributeAsync(IDistributionReporter reporter, CancellationToken cancellationToken = default);

    void ExportPartDistribution();

    #endregion
}