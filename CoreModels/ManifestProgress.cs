using System.Windows.Forms;

namespace Aspire.CoreModels
{
	public partial class ManifestProgress : Form
	{
		public ManifestProgress(int numSteps)
		{
			InitializeComponent();
			progressBar1.Maximum = numSteps;
			progressBar1.Step = 1;
		}

		public void Increment()
		{
			progressBar1.PerformStep();
		}

	}
}
