using System.Threading.Tasks;
using LogJoint.Postprocessing;
using System.Linq;
using System;
using NFluent;

namespace LogJoint.Tests.Integration.Symphony
{
	[IntegrationTestFixture]
	class SymphonyRtcLogTests
	{
		[IntegrationTest]
		public async Task LoadsLogFileAndEnablesPostprocessors(IContext context)
		{
			await context.Utils.EmulateFileDragAndDrop(await context.Samples.GetSampleAsLocalFile("rtcLog2018-09-15T21_41_44.195Z.txt"));

			await context.Utils.WaitFor(() => context.Presentation.LoadedMessagesLogViewer.VisibleLines.Count > 0);
			Check.That(context.Presentation.LoadedMessagesLogViewer.VisibleLines[0].Value).IsEqualTo(
				"2018-09-15T21:41:34.133Z | TRACE(4) | stats-1 | RTCMediaStreamTrack_receiver_21.totalSamplesDuration=0");

			var postprocessorsControls = context.Presentation.Postprocessing.SummaryView;
			Check.That(postprocessorsControls.Timeline.Enabled).IsTrue();
			Check.That(postprocessorsControls.SequenceDiagram.Enabled).IsTrue();
			Check.That(postprocessorsControls.StateInspector.Enabled).IsTrue();
			Check.That(postprocessorsControls.TimeSeries.Enabled).IsTrue();
		}

		[IntegrationTest]
		public async Task CanRunStateInspectorPostprocessor(IContext context)
		{
			await context.Utils.EmulateFileDragAndDrop(await context.Samples.GetSampleAsLocalFile("rtcLog2018-09-15T21_41_44.195Z.txt"));

			var postprocessorsControls = context.Presentation.Postprocessing.SummaryView;
			await context.Utils.WaitFor(() => postprocessorsControls.StateInspector.Run != null);

			postprocessorsControls.StateInspector.Run();

			await context.Utils.WaitFor(() => postprocessorsControls.StateInspector.Show != null);

			var symRtc = await context.Presentation.Postprocessing.StateInspector.Roots.FirstOrDefault(n => n.Id == "Symphony RTC");
			Check.That(symRtc).IsNotNull();
			var meeting1 = await symRtc.Children.FirstOrDefault(n => n.Id == "meeting-1");
			Check.That(meeting1).IsNotNull();
			var session1 = await meeting1.Children.FirstOrDefault(n => n.Id == "session-1");
			Check.That(session1).IsNotNull();
			var sessionIdChange = session1.ChangeHistory.FirstOrDefault(i => i.PropertyName == "session id");
			Check.That(sessionIdChange).IsNotNull();
			Check.That(sessionIdChange.Value).IsEqualTo("6357b48c-89c4-45d1-80c0-0a98245f7d6c");
		}

		[IntegrationTest]
		public async Task CanCorrelateClientLogsFromChromedebugLogAndCSLogs(IContext context)
		{
			await context.Utils.EmulateFileDragAndDrop(await context.Samples.GetSampleAsLocalFile("sym-correlate-a5b9dd80-1daf-43a6-8f76-b5691c121dfc.zip"));

			await context.Utils.WaitFor(() => context.Model.SourcesManager.VisibleItems.Count == 3);
			var logs = context.Model.SourcesManager.VisibleItems;
			var chromeDebugLog = logs.FirstOrDefault(l => l.DisplayName.Contains("chrome_debug"));
			var csLog1 = logs.FirstOrDefault(l => l.DisplayName.Contains("6ef29e77-e68a-4c78-b440-f5d31ea69614"));
			var csLog2 = logs.FirstOrDefault(l => l.DisplayName.Contains("06708cd4-ef81-4862-82db-eebbcd33eaba"));
			Check.That(chromeDebugLog).IsNotNull();
			Check.That(csLog1).IsNotNull();
			Check.That(csLog2).IsNotNull();

			var postprocessorsControls = context.Presentation.Postprocessing.SummaryView;
			await context.Utils.WaitFor(() => postprocessorsControls.Correlation.Run != null);

			postprocessorsControls.Correlation.Run();

			await context.Utils.WaitFor(() => csLog1.TimeOffsets.BaseOffset != TimeSpan.Zero);

			Check.That(csLog1.TimeOffsets.BaseOffset.TotalMinutes).IsCloseTo(120, 0.01);
			Check.That(csLog2.TimeOffsets.BaseOffset.TotalMinutes).IsCloseTo(120, 0.01);
			Check.That(chromeDebugLog.TimeOffsets.BaseOffset.TotalMinutes).IsCloseTo(0, 0.01);
		}
	}
}
