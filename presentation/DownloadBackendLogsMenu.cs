
using System;
using System.Text.RegularExpressions;
using SI = LogJoint.UI.Presenters.Postprocessing.StateInspectorVisualizer;

namespace LogJoint.Symphony.UI.Presenters
{
    static class DownloadBackendLogsMenu
    {
        public static SI.MenuData.Item CreateStateInspectorMenuItem(
            SI.IVisualizerNode selectedNode,
            LogJoint.UI.Presenters.IPromptDialog prompt,
            Preprocessing.IManager preprocessingManager,
            SpringServiceLog.IPreprocessingStepsFactory preprocessingStepsFactory
        )
        {
            SI.IVisualizerNode GetParent(SI.IVisualizerNode n) => n.Parent == null ? n : GetParent(n.Parent);
            var (id, referenceTime, env) = Rtc.MeetingsStateInspector.GetMeetingRelatedId(
                selectedNode.CreationEvent, selectedNode.ChangeHistory,
                GetParent(selectedNode).CreationEvent, GetParent(selectedNode).ChangeHistory
            );
            if (id != null)
            {
                return new SI.MenuData.Item()
                {
                    Text = "Download backend logs",
                    Click = () =>
                    {
                        var input = prompt.ExecuteDialog(
                            "Download RTC backend logs",
                            "Specify query parameters",
                            $"ID={id}{Environment.NewLine}Environment={env ?? "(undetected)"}{Environment.NewLine}Reference time={referenceTime.ToString("o")}");
                        if (input != null)
                        {
                            var ids = new[] { id };
                            foreach (var line in input.Split('\r', '\n'))
                            {
                                var m = Regex.Match(line, @"^(?<k>[^\=]+)\=(?<v>.+)$", RegexOptions.ExplicitCapture);
                                if (!m.Success)
                                    continue;
                                var k = m.Groups["k"].Value;
                                var v = m.Groups["v"].Value;
                                if (k == "ID")
                                    ids = v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                else if (k == "Environment")
                                    env = v;
                                else if (k == "Reference time")
                                    if (DateTime.TryParseExact(v, "o", null, System.Globalization.DateTimeStyles.None, out var tmpRefTime))
                                        referenceTime = tmpRefTime;
                            }
                            preprocessingManager.Preprocess(
                                new[] { preprocessingStepsFactory.CreateDownloadBackendLogsStep(ids, referenceTime, env) },
                                "Downloading backend logs",
                                Preprocessing.PreprocessingOptions.HighlightNewPreprocessing
                            );
                        }
                    }
                };
            }
            return null;
        }
    }
}
