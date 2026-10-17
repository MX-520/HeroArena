using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>Prepare and inspect candidate clips only; does not add playback behavior or touch scenes.</summary>
public static class AudioCandidatePreparation
{
    [Serializable] private class ClipInfo
    {
        public string path;
        public float seconds;
        public int sampleRate,channels,samples;
        public string loadType;
    }
    [Serializable] private class Report { public string status; public ClipInfo[] clips; }

    public static void ImportAndVerify()
    {
        try
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var paths=AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Audio/Candidates"}).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p=>p).ToArray();
            if(paths.Length!=24) throw new Exception("Expected 24 candidate audio clips, got "+paths.Length);
            var report=new List<ClipInfo>();
            foreach(var path in paths)
            {
                var importer=(AudioImporter)AssetImporter.GetAtPath(path);
                bool music=path.Contains("/CleytonKauffman/");
                importer.forceToMono=!music;importer.loadInBackground=music;
                var settings=importer.defaultSampleSettings;
                settings.preloadAudioData=!music;
                settings.loadType=music?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat=music?AudioCompressionFormat.Vorbis:AudioCompressionFormat.PCM;
                settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
                settings.quality=0.8f;importer.defaultSampleSettings=settings;
                importer.SaveAndReimport();
                var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if(clip==null || clip.length<=0 || clip.samples<=0 || clip.channels<1 || clip.frequency<8000)
                    throw new Exception("Audio import/metadata failed: "+path);
                report.Add(new ClipInfo{path=path,seconds=clip.length,sampleRate=clip.frequency,channels=clip.channels,samples=clip.samples,loadType=settings.loadType.ToString()});
            }
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/audio-candidates-report.json",JsonUtility.ToJson(new Report{status="PASS: candidates imported; not integrated or audition-approved",clips=report.ToArray()},true));
            AssetDatabase.SaveAssets();Debug.Log("PASS audio-candidates: 24 clips imported; no playback integration");
            EditorApplication.Exit(0);
        }
        catch(Exception e) { Debug.LogException(e);EditorApplication.Exit(1); }
    }
}
