using System.Threading.Tasks;
using System.IO;
using System.Threading;
using NFluent;

namespace LogJoint.Tests.Integration.Symphony
{
	[IntegrationTestFixture]
	class SpringServiceLogFormatTests
	{
		[IntegrationTest]
		public async Task LoadsLogFileAndEnablesPostprocessors(IContext context)
		{
			await context.Utils.EmulateFileDragAndDrop(await context.Samples.GetSampleAsLocalFile("SpringServiceLog-06708cd4-ef81-4862-82db-eebbcd33eaba.log"));

			await context.Utils.WaitFor(() => context.Presentation.LoadedMessagesLogViewer.VisibleLines.Count > 0);

			Check.That(context.Presentation.LoadedMessagesLogViewer.VisibleLines[0].Value).IsEqualTo(
				"leave sessionId 8f7c5ff0-3b88-4bbb-98ca-dd36da312a63 trigger LEAVE_FROM_CLIENT");
			var postprocessorsControls = context.Presentation.Postprocessing.SummaryView;
			Check.That(postprocessorsControls.Timeline.Enabled).IsTrue();
			Check.That(postprocessorsControls.SequenceDiagram.Enabled).IsTrue();
		}
	}
}
