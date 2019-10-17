﻿using LogJoint.Postprocessing;
using LogJoint.Postprocessing.Timeline;
using System.Threading;
using System.Threading.Tasks;
using SVC = LogJoint.Symphony.SpringServiceLog;
using Sym = LogJoint.Symphony.Rtc;

namespace LogJoint.Symphony.Timeline
{
	public interface IPostprocessorsFactory
	{
		ILogSourcePostprocessor CreateSpringServiceLogPostprocessor();
		ILogSourcePostprocessor CreateSymRtcPostprocessor();
		Chromium.EventsSource<Event, MessagePrefixesPair<Chromium.ChromeDriver.Message>>.Factory CreateChromeDriverEventsSourceFactory();
		Chromium.EventsSource<Event, Chromium.ChromeDebugLog.Message>.Factory CreateChromeDebugLogEventsSourceFactory();
	};

	public class PostprocessorsFactory : IPostprocessorsFactory
	{
		readonly ITempFilesManager tempFiles;
		readonly Postprocessing.IModel postprocessing;

		public PostprocessorsFactory(
			ITempFilesManager tempFiles,
			Postprocessing.IModel postprocessing)
		{
			this.tempFiles = tempFiles;
			this.postprocessing = postprocessing;
		}

		ILogSourcePostprocessor IPostprocessorsFactory.CreateSpringServiceLogPostprocessor()
		{
			return new LogSourcePostprocessor(
				PostprocessorKind.Timeline,
				i => RunForSpringServiceLog(new SVC.Reader(postprocessing.TextLogParser, i.CancellationToken).Read(
					i.LogFileName, i.ProgressHandler), i)
			);
		}

		ILogSourcePostprocessor IPostprocessorsFactory.CreateSymRtcPostprocessor()
		{
			return new LogSourcePostprocessor(
				PostprocessorKind.Timeline,
				i => RunForSymLog(new Sym.Reader(postprocessing.TextLogParser, i.CancellationToken).Read(i.LogFileName, i.ProgressHandler), i)
			);
		}

		Chromium.EventsSource<Event, MessagePrefixesPair<Chromium.ChromeDriver.Message>>.Factory IPostprocessorsFactory.CreateChromeDriverEventsSourceFactory()
		{
			return (matcher, messages, tracker) =>
			{
				Sym.ICITimelineEvents symCIEvents = new Sym.CITimelineEvents(matcher);

				return new Chromium.EventsSource<Event, MessagePrefixesPair<Chromium.ChromeDriver.Message>>(symCIEvents.GetEvents(messages));
			};
		}

		Chromium.EventsSource<Event, Chromium.ChromeDebugLog.Message>.Factory IPostprocessorsFactory.CreateChromeDebugLogEventsSourceFactory()
		{
			return (matcher, messages, tracker) =>
			{
				Sym.ICITimelineEvents symCI = new Sym.CITimelineEvents(matcher);

				var symEvents = RunForSymMessages(
					matcher,
					(new Sym.Reader(postprocessing.TextLogParser, CancellationToken.None)).FromChromeDebugLog(messages),
					tracker,
					out var symLog
				);
				var ciEvents = symCI.GetEvents(messages);

				var events = EnumerableAsync.Merge(
					symEvents,
					ciEvents
				);

				return new Chromium.EventsSource<Event, Chromium.ChromeDebugLog.Message>(events, symLog);
			};
		}

		async Task RunForSpringServiceLog(
			IEnumerableAsync<SVC.Message[]> input,
			LogSourcePostprocessorInput postprocessorInput
		)
		{
			var inputMultiplexed = input.Multiplex();

			var messagingEventsSource = postprocessing.Timeline.CreateMessagingEventsSource();
			var messagingEvents = messagingEventsSource.GetEvents(
				((SVC.IMessagingEvents)new SVC.MessagingEvents()).GetEvents(inputMultiplexed));
			var eofEvents = postprocessing.Timeline.CreateEndOfTimelineEventSource<SVC.Message>()
				.GetEvents(inputMultiplexed);
				
			var events = EnumerableAsync.Merge(
				messagingEvents,
				eofEvents
			);

			var serialize = postprocessing.Timeline.SavePostprocessorOutput(
				events,
				null,
				evtTrigger => TextLogEventTrigger.Make((SVC.Message)evtTrigger),
				postprocessorInput
			);

			await Task.WhenAll(serialize, inputMultiplexed.Open());
		}

		async Task RunForSymLog(
			IEnumerableAsync<Sym.Message[]> input,
			LogSourcePostprocessorInput postprocessorInput
		)
		{
			IPrefixMatcher matcher = postprocessing.CreatePrefixMatcher();
			var inputMultiplexed = input.Multiplex();
			var symEvents = RunForSymMessages(matcher, inputMultiplexed, postprocessorInput.TemplatesTracker, out var symLog);
			var endOfTimelineEventSource = postprocessing.Timeline.CreateEndOfTimelineEventSource<Sym.Message>();
			var messagingEventsSource = postprocessing.Timeline.CreateMessagingEventsSource();
			var messagingEvents = messagingEventsSource.GetEvents(
				((Sym.IMessagingEvents)new Sym.MessagingEvents()).GetEvents(inputMultiplexed));
			var eofEvts = endOfTimelineEventSource.GetEvents(inputMultiplexed);

			matcher.Freeze();

			var events = EnumerableAsync.Merge(
				symEvents,
				messagingEvents,
				eofEvts
			);

			var serialize = postprocessing.Timeline.SavePostprocessorOutput(
				events,
				null,
				evtTrigger => TextLogEventTrigger.Make((Sym.Message)evtTrigger),
				postprocessorInput
			);

			await Task.WhenAll(serialize, symLog.Open(), inputMultiplexed.Open());
		}

		private IEnumerableAsync<Event[]> RunForSymMessages(
			IPrefixMatcher matcher,
			IEnumerableAsync<Sym.Message[]> messages,
			ICodepathTracker templatesTracker,
			out IMultiplexingEnumerable<MessagePrefixesPair<Sym.Message>[]> symLog
		)
		{
			Sym.IMeetingsStateInspector symMeetingsStateInspector = new Sym.MeetingsStateInspector(matcher);
			Sym.IMediaStateInspector symMediaStateInsector = new Sym.MediaStateInspector(matcher, symMeetingsStateInspector);
			Sym.ITimelineEvents symTimelineEvents = new Sym.TimelineEvents(matcher);
			Sym.Diag.ITimelineEvents diagTimelineEvents = new Sym.Diag.TimelineEvents(matcher);

			symLog = messages.MatchTextPrefixes(matcher).Multiplex();
			var symMeetingStateEvents = symMeetingsStateInspector.GetEvents(symLog);
			var symMediaStateEvents = symMediaStateInsector.GetEvents(symLog);

			var symMeetingEvents = postprocessing.Timeline.CreateInspectedObjectsLifetimeEventsSource(e =>
				e.ObjectType == Sym.MeetingsStateInspector.MeetingTypeInfo
			 || e.ObjectType == Sym.MeetingsStateInspector.MeetingSessionTypeInfo
			 || e.ObjectType == Sym.MeetingsStateInspector.MeetingRemoteParticipantTypeInfo
			 || e.ObjectType == Sym.MeetingsStateInspector.ProbeSessionTypeInfo
			 || e.ObjectType == Sym.MeetingsStateInspector.InvitationTypeInfo
			).GetEvents(symMeetingStateEvents);

			var symMediaEvents = postprocessing.Timeline.CreateInspectedObjectsLifetimeEventsSource(e =>
			   	e.ObjectType == Sym.MediaStateInspector.LocalScreenTypeInfo
			 || e.ObjectType == Sym.MediaStateInspector.LocalAudioTypeInfo
			 || e.ObjectType == Sym.MediaStateInspector.LocalVideoTypeInfo
			 || e.ObjectType == Sym.MediaStateInspector.TestSessionTypeInfo
			).GetEvents(symMediaStateEvents);

			var events = templatesTracker.TrackTemplates(EnumerableAsync.Merge(
				symMeetingEvents,
				symMediaEvents,
				symTimelineEvents.GetEvents(symLog),
				diagTimelineEvents.GetEvents(symLog)
			));

			return events;
		}
	};
}
