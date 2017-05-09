using System;
using System.IO;
using UnityEngine;
using UnityEngine.Recorder.FrameRecorder;
using UnityEngine.Recorder.FrameRecorder.DataSource;

namespace UTJ.FrameCapturer.Recorders
{
    [FrameRecorderClass]
    public class GIFRecorder : RenderTextureRecorder<GIFRecorderSettings>
    {
        fcAPI.fcGifContext m_ctx;
        fcAPI.fcStream m_stream;

        public static RecorderInfo GetRecorderInfo()
        {
            return RecorderInfo.Instantiate<GIFRecorder, GIFRecorderSettings>("Video", "GIF Recorder");
        }

        public override bool BeginRecording(RecordingSession session)
        {
            if (!base.BeginRecording(session)) { return false; }

            if (!Directory.Exists(m_Settings.m_DestinationPath))
                Directory.CreateDirectory(m_Settings.m_DestinationPath);

            return true;
        }

        public override void EndRecording(RecordingSession session)
        {
            m_ctx.Release();
            m_stream.Release();
            base.EndRecording(session);
        }

        public override void RecordFrame(RecordingSession session)
        {
            if (m_BoxedSources.Count != 1)
                throw new Exception("Unsupported number of sources");

            var source = (RenderTextureSource)m_BoxedSources[0].m_Source;
            var frame = source.buffer;

            if(!m_ctx)
            {
                var settings = m_Settings.m_GifEncoderSettings;
                settings.width = frame.width;
                settings.height = frame.height;
                m_ctx = fcAPI.fcGifCreateContext(ref settings);
                m_stream = fcAPI.fcCreateFileStream(BuildOutputPath(session));
                fcAPI.fcGifAddOutputStream(m_ctx, m_stream);
            }

            fcAPI.fcLock(frame, TextureFormat.RGB24, (data, fmt) =>
            {
                fcAPI.fcGifAddFramePixels(m_ctx, data, fmt, session.m_CurrentFrameStartTS);
            });
        }

        string BuildOutputPath(RecordingSession session)
        {
            var outputPath = m_Settings.m_DestinationPath;
            if (outputPath.Length > 0 && !outputPath.EndsWith("/"))
                outputPath += "/";
            outputPath += m_OutputFile + (settings as GIFRecorderSettings).m_BaseFileName + ".gif";
            return outputPath;
        }
    }
}
