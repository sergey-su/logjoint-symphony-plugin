
using System;
using System.Text.RegularExpressions;
using SI = LogJoint.UI.Presenters.Postprocessing.StateInspectorVisualizer;
using TS = LogJoint.UI.Presenters.Postprocessing.TimeSeriesVisualizer;

namespace LogJoint.Symphony.UI.Presenters
{
    static class GoToTimeSeriesMenu
    {
        public static SI.MenuData.Item CreateStateInspectorMenuItem(
            SI.IVisualizerNode selectedNode,
            TS.IPresenter timeSeriesPresenter
        )
        {
            if (Rtc.MediaStateInspector.HasTimeSeries(selectedNode.CreationEvent))
            {
                bool predicate(LogJoint.UI.Presenters.Postprocessing.TimeSeriesVisualizer.ITreeNodeData node) =>
                        node.Type == LogJoint.UI.Presenters.Postprocessing.TimeSeriesVisualizer.ConfigDialogNodeType.ObjectIdGroup
                    && node.Caption.Contains(selectedNode.Id)
                    && selectedNode.BelongsToSource(node.LogSource);
                if (timeSeriesPresenter.ConfigNodeExists(predicate))
                {
                    return new SI.MenuData.Item(
                        "Go to time series",
                        () =>
                        {
                            timeSeriesPresenter.Show();
                            timeSeriesPresenter.OpenConfigDialog();
                            timeSeriesPresenter.SelectConfigNode(predicate);
                        }
                    );
                }
            }
            return null;
        }
    }
}