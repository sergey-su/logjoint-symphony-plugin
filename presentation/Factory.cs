using System.Linq;

namespace LogJoint.Symphony.UI.Presenters
{
	public class Factory
	{
		static public void Create(IModel appModel, ModelObjects modelObjects, LogJoint.UI.Presenters.IPresentation appPresentation)
		{
			var stateInspectorPresenter = appPresentation.Postprocessing.StateInspector;

			stateInspectorPresenter.OnNodeCreated += (senderPresenter, arg) =>
			{
				if (Rtc.MeetingsStateInspector.ShouldBePresentedCollapsed(arg.NodeObject?.CreationEvent))
					arg.CreateCollapsed = true;
				else if (Rtc.MediaStateInspector.ShouldBePresentedCollapsed(arg.NodeObject?.CreationEvent))
					arg.CreateCollapsed = true;
			};

			stateInspectorPresenter.OnMenu += (senderPresenter, arg) =>
			{
				if (stateInspectorPresenter.SelectedObject != null)
				{
					var menuItems = new []
					{
						GoToTimeSeriesMenu.CreateStateInspectorMenuItem(
							stateInspectorPresenter.SelectedObject,
							appPresentation.Postprocessing.TimeSeries),
						DownloadBackendLogsMenu.CreateStateInspectorMenuItem(
							stateInspectorPresenter.SelectedObject,
							appPresentation.PromptDialog, appModel.Preprocessing.Manager,modelObjects.BackendLogsPreprocessingStepsFactory)
					};

					arg.Items.AddRange(menuItems.Where(i => i != null));
				}
			};
		}
	}
}
