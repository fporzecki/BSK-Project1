using System.ComponentModel;
using System.Threading;

namespace BSK_Proj1_Logic
{
    public class TempClass
    {
        private readonly BackgroundWorker _backgroundWorker;

        public TempClass(BackgroundWorker backgroundWorker)
        {
            _backgroundWorker = backgroundWorker;
        }

        public void EncryptFile()
        {
            var result = 0;
            for (var i = 0; i <= 100; i++)
            {
                result = i;
                Thread.Sleep(100);
                _backgroundWorker.ReportProgress(i);
            }
        }
    }
}
