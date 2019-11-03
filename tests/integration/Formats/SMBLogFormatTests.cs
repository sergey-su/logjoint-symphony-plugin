using System.Threading.Tasks;
using System.IO;
using System.Threading;
using NFluent;

namespace LogJoint.Tests.Integration.Symphony
{
	[IntegrationTestFixture]
	class SMBLogFormatTests
	{
		[IntegrationTest]
		public async Task LoadsLogFileAndEnablesPostprocessors(IContext context)
		{
			await context.Utils.EmulateFileDragAndDrop(await context.Samples.GetSampleAsLocalFile("smb2019-09-13.log"));

			await context.Utils.WaitFor(() => context.Presentation.LoadedMessagesLogViewer.VisibleLines.Count > 0);

			Check.That(context.Presentation.LoadedMessagesLogViewer.VisibleLines[19].Value).IsEqualTo(
				"2019-09-13 17:28:07.646 INFO [0x70000f297000][Transport-7] NICE_COMPONENT_STATE_FAILED");
			var postprocessorsControls = context.Presentation.Postprocessing.SummaryView;
			Check.That(postprocessorsControls.SequenceDiagram.Enabled).IsTrue();
		}
	}
}
