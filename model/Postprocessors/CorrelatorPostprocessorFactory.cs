using System.Threading.Tasks;
using System.Linq;
using LogJoint.Postprocessing;
using LogJoint.Postprocessing.Correlation;
using M = LogJoint.Postprocessing.Messaging;
using System.Collections.Generic;
using System.Text;
using System;
using SVC = LogJoint.Symphony.SpringServiceLog;

namespace LogJoint.Symphony.Correlator
{
	public interface IPostprocessorsFactory
	{
		ILogSourcePostprocessor CreateSpringServiceLogPostprocessor();
	};

	public class PostprocessorsFactory : IPostprocessorsFactory
	{
		readonly Postprocessing.IModel postprocessingModel;

		public PostprocessorsFactory(IModel ljModel)
		{
			this.postprocessingModel = ljModel.Postprocessing;
		}

		ILogSourcePostprocessor IPostprocessorsFactory.CreateSpringServiceLogPostprocessor()
		{
			return new LogSourcePostprocessor(PostprocessorKind.Correlator, RunForSpringServiceLog);
		}

		async Task RunForSpringServiceLog(LogSourcePostprocessorInput input)
		{
			var reader = new SpringServiceLog.Reader(postprocessingModel.TextLogParser, input.CancellationToken).Read(input.LogFileName, input.ProgressHandler);

			SVC.IMessagingEvents messagingEvents = new SVC.MessagingEvents();

			var events = EnumerableAsync.Merge(
				messagingEvents.GetEvents(reader)
			);

			await postprocessingModel.Correlation.CreatePostprocessorOutputBuilder()
				.SetMessagingEvents(events)
				.SetTriggersConverter(evtTrigger => TextLogEventTrigger.Make((SVC.Message)evtTrigger))
				.Build(input);
		}
	}
}
