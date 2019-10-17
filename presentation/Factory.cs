using System;
using System.Text.RegularExpressions;
using SI = LogJoint.UI.Presenters.Postprocessing.StateInspectorVisualizer;

namespace LogJoint.Symphony.UI.Presenters
{
	public class Factory
	{
		static public void Create(IModel appModel, ModelObjects modelObjects, LogJoint.UI.Presenters.IPresentation appPresentation)
		{
			LogJoint.UI.Presenters.Postprocessing.TimeSeriesVisualizer.IPresenter timeSeriesPresenter = null;
			LogJoint.UI.Presenters.Postprocessing.IPostprocessorOutputForm timeSeriesForm = null;

			appPresentation.PostprocessorsFormFactory.FormCreated += (sender, evt) =>
			{
				if (evt.Id == LogJoint.UI.Presenters.Postprocessing.MainWindowTabPage.ViewControlId.StateInspector)
				{
					if (evt.Presenter is SI.IPresenter stateInspectorPresenter)
					{
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
								if (Rtc.MediaStateInspector.HasTimeSeries(stateInspectorPresenter.SelectedObject.CreationEvent))
								{
									appPresentation.PostprocessorsFormFactory.GetPostprocessorOutputForm(LogJoint.UI.Presenters.Postprocessing.MainWindowTabPage.ViewControlId.TimeSeries);
									bool predicate(LogJoint.UI.Presenters.Postprocessing.TimeSeriesVisualizer.ITreeNodeData node) =>
										   node.Type == LogJoint.UI.Presenters.Postprocessing.TimeSeriesVisualizer.ConfigDialogNodeType.ObjectIdGroup
										&& node.Caption.Contains(stateInspectorPresenter.SelectedObject.Id)
										&& stateInspectorPresenter.SelectedObject.BelongsToSource(node.LogSource);
									if (timeSeriesPresenter != null && timeSeriesPresenter.ConfigNodeExists(predicate))
									{
										arg.Items.Add(new SI.MenuData.Item()
										{
											Text = "Go to time series",
											Click = () =>
											{
												timeSeriesForm.Show();
												timeSeriesPresenter.OpenConfigDialog();
												timeSeriesPresenter.SelectConfigNode(predicate);
											}
										});
									}
								}

								if (modelObjects.BackendLogsPreprocessingStepsFactory != null)
								{
									var menuItem = DownloadBackendLogsMenu.CreateStateInspectorMenuItem(stateInspectorPresenter.SelectedObject,
										appPresentation.PromptDialog, appModel.Preprocessing.Manager, modelObjects.BackendLogsPreprocessingStepsFactory);
									if (menuItem != null)
										arg.Items.Add(menuItem);
								}
							}
						};
					}
				}
				else if (evt.Id == LogJoint.UI.Presenters.Postprocessing.MainWindowTabPage.ViewControlId.TimeSeries)
				{
					timeSeriesPresenter = evt.Presenter as LogJoint.UI.Presenters.Postprocessing.TimeSeriesVisualizer.IPresenter;
					timeSeriesForm = evt.Form;
				}
			};
		}
	}
}
