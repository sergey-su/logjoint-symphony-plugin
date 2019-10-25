﻿using System.Linq;
using LogJoint.Postprocessing;
using System;

namespace LogJoint.Symphony
{
	public interface IPostprocessorsRegistry
	{
		LogSourceMetadata SpringServiceLog { get; }
		LogSourceMetadata RtcLog { get; }
		LogSourceMetadata SMBLog { get; }
	};

	public class PostprocessorsInitializer : IPostprocessorsRegistry
	{
		private readonly IUserDefinedFactory symRtcLogFormat, springServiceLogFormat, smbLogFormat;
		private readonly LogSourceMetadata symRtcLogMeta, springServiceLogMeta, smbLogMeta;


		public PostprocessorsInitializer(
			IManager postprocessorsManager,
			IUserDefinedFormatsManager userDefinedFormatsManager,
			StateInspector.IPostprocessorsFactory stateInspectorPostprocessorsFactory,
			TimeSeries.IPostprocessorsFactory timeSeriesPostprocessorsFactory,
			Correlator.IPostprocessorsFactory correlatorPostprocessorsFactory,
			Timeline.IPostprocessorsFactory timelinePostprocessorsFactory,
			SequenceDiagram.IPostprocessorsFactory sequenceDiagramPostprocessorsFactory
		)
		{
			IUserDefinedFactory findFormat(string company, string formatName)
			{
				var ret = userDefinedFormatsManager.Items.FirstOrDefault(
					f => f.CompanyName == company && f.FormatName == formatName);
				if (ret == null)
					throw new Exception(string.Format("Log format {0} is not registered in LogJoint", formatName));
				return ret;
			}

			this.symRtcLogFormat = findFormat("Symphony", "RTC log");
			this.springServiceLogFormat = findFormat("Symphony", "RTC Java Spring Service log");
			this.smbLogFormat = findFormat("Symphony", "SMB log");

			this.springServiceLogMeta = new LogSourceMetadata(
				springServiceLogFormat,
				sequenceDiagramPostprocessorsFactory.CreateSpringServiceLogPostprocessor(),
				timelinePostprocessorsFactory.CreateSpringServiceLogPostprocessor(),
				correlatorPostprocessorsFactory.CreateSpringServiceLogPostprocessor()
			);
			postprocessorsManager.Register(this.springServiceLogMeta);

			this.symRtcLogMeta = new LogSourceMetadata(
				symRtcLogFormat,
				stateInspectorPostprocessorsFactory.CreateSymphonyRtcPostprocessor(),
				timeSeriesPostprocessorsFactory.CreateSymphonyRtcPostprocessor(),
				timelinePostprocessorsFactory.CreateSymRtcPostprocessor(),
				sequenceDiagramPostprocessorsFactory.CreateRtcLogPostprocessor()
			);
			postprocessorsManager.Register(this.symRtcLogMeta);

			this.smbLogMeta = new LogSourceMetadata(
				smbLogFormat,
				sequenceDiagramPostprocessorsFactory.CreateSMBPostprocessor()
			);
			postprocessorsManager.Register(this.smbLogMeta);
		}

		LogSourceMetadata IPostprocessorsRegistry.SpringServiceLog => springServiceLogMeta;

		LogSourceMetadata IPostprocessorsRegistry.RtcLog => symRtcLogMeta;

		LogSourceMetadata IPostprocessorsRegistry.SMBLog => smbLogMeta;
	};
}
